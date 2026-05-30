# Reset -> AutoStart validation (Production post-import). No UI/index checks.
param(
    [string]$BaseUrl = 'https://localhost',
    [string]$Username = 'admin',
    [string]$Password = 'Admin@123456',
    [string]$ProdConfigPath = 'C:\inetpub\wwwroot\shared\appsettings.Production.json',
    [string]$OutDir = '',
    [switch]$SimulatePbtMarginProbe
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

if (-not $OutDir) {
    $OutDir = Join-Path $PSScriptRoot "exports\validation\reset_autostart_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
}
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

$checks = New-Object System.Collections.Generic.List[object]

function Add-Check($Id, $Name, [bool]$Pass, $Detail, $Evidence = $null) {
    $checks.Add([ordered]@{
        id = $Id; name = $Name; pass = $Pass; status = if ($Pass) { 'PASS' } else { 'FAIL' }
        detail = $Detail; evidence = $Evidence; atUtc = (Get-Date).ToUniversalTime().ToString('o')
    }) | Out-Null
}

function Open-Conn($Cs) { $c = New-Object System.Data.SqlClient.SqlConnection $Cs; $c.Open(); return $c }
function Get-Scalar($Conn, $Sql) { $cmd = $Conn.CreateCommand(); $cmd.CommandText = $Sql; return $cmd.ExecuteScalar() }
function Invoke-Api {
    param([string]$Method, [string]$Path, [hashtable]$Headers = @{}, $Body = $null)
    $uri = "$BaseUrl$Path"
    if ($Body) { return Invoke-RestMethod -Uri $uri -Method $Method -Headers $Headers -ContentType 'application/json' -Body ($Body | ConvertTo-Json) -TimeoutSec 90 }
    return Invoke-RestMethod -Uri $uri -Method $Method -Headers $Headers -TimeoutSec 90
}
function Unwrap($o) { if ($o.data) { return $o.data }; return $o }
function Fmt-SqlDate($Value) {
    if ($null -eq $Value -or $Value -is [DBNull]) { return $null }
    return ([datetime]$Value).ToString('yyyy-MM-dd HH:mm:ss')
}

$config = Get-Content -LiteralPath $ProdConfigPath -Raw | ConvertFrom-Json
$conn = Open-Conn ([string]$config.ConnectionStrings.DefaultConnection)

$baseline = [ordered]@{
    sessions = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionSessions')
    samples = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionMarginSamples')
    open = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionSessions WHERE Completed = 0')
    histSeq = [int](Get-Scalar $conn "SELECT COUNT(*) FROM dbo.MissionSessions WHERE MissionKey LIKE 'hist-seq-%'")
    live18 = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionSessions WHERE ID BETWEEN 1 AND 8')
}

Add-Check 'PRE-01' 'Baseline: 27 sessions, 48856 samples, 0 open' `
    ($baseline.sessions -eq 27 -and $baseline.samples -eq 48856 -and $baseline.open -eq 0) `
    "sessions=$($baseline.sessions) samples=$($baseline.samples) open=$($baseline.open)" $baseline

if ($baseline.open -gt 0) { throw "Refusing reset probe: $($baseline.open) open mission(s)" }

$login = Unwrap (Invoke-Api -Method POST -Path '/api/Auth/login' -Headers @{} -Body @{ username = $Username; password = $Password })
$token = if ($login.token) { $login.token } else { $login.Token }
Add-Check 'RST-01' 'API login' ([bool]$token) "tokenLength=$($token.Length)"

$h = @{ Authorization = "Bearer $token" }

# alreadyTracked simulation BEFORE reset (with latest margin after last reset)
$resetBefore = Get-Scalar $conn "SELECT TOP 1 Value FROM dbo.Configurations WHERE [K] = 'MISSION_LAST_RESET_AT_UTC'"
$fpBefore = $null
if ($resetBefore -and $resetBefore -isnot [DBNull]) {
    $fpBefore = Get-Scalar $conn "SELECT MIN(Data) FROM dbo.Margini WHERE Data > '$(Fmt-SqlDate $resetBefore)'"
}
$blockHist = 0
if ($fpBefore -and $fpBefore -isnot [DBNull]) {
    $fp = ([datetime]$fpBefore).ToString('yyyy-MM-dd HH:mm:ss')
    $blockHist = [int](Get-Scalar $conn @"
SELECT COUNT(*) FROM dbo.MissionSessions WHERE StartTime >= '$fp' AND MissionKey LIKE 'hist-seq-%'
"@)
}
Add-Check 'GRD-01' 'hist-seq do not satisfy alreadyTracked StartTime gate' ($blockHist -eq 0) `
    "blockingHist=$blockHist firstPoint=$fpBefore"

# Reset dashboard
$resetResp = Unwrap (Invoke-Api -Method POST -Path '/api/decider/reset' -Headers $h)
Add-Check 'RST-02' 'POST /api/decider/reset' ($null -ne $resetResp) 'Reset OK' $resetResp

Start-Sleep -Seconds 2
$resetAfter = Get-Scalar $conn "SELECT TOP 1 Value FROM dbo.Configurations WHERE [K] = 'MISSION_LAST_RESET_AT_UTC'"
Add-Check 'RST-03' 'Reset boundary updated' ($resetAfter -isnot [DBNull] -and [string]$resetAfter -ne [string]$resetBefore) `
    "before=$resetBefore after=$resetAfter"

$sessionsAfterReset = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionSessions')
$histAfterReset = [int](Get-Scalar $conn "SELECT COUNT(*) FROM dbo.MissionSessions WHERE MissionKey LIKE 'hist-seq-%'")
Add-Check 'RST-04' 'Production rows unchanged after reset' `
    ($sessionsAfterReset -eq $baseline.sessions -and $histAfterReset -eq $baseline.histSeq) `
    "sessions=$sessionsAfterReset histSeq=$histAfterReset"

# Simulate first PBT margin point after reset (probe row, removed in cleanup)
$probeMarginId = $null
if (-not $SimulatePbtMarginProbe.IsPresent) { $SimulatePbtMarginProbe = $true }
if ($SimulatePbtMarginProbe) {
    $probeTs = (Get-Date).ToUniversalTime()
    $ins = $conn.CreateCommand()
    $ins.CommandText = 'INSERT INTO dbo.Margini (Margine, Data) OUTPUT INSERTED.Id VALUES (@m, @d)'
    [void]$ins.Parameters.AddWithValue('@m', 0.01)
    [void]$ins.Parameters.AddWithValue('@d', $probeTs)
    $probeMarginId = [int]$ins.ExecuteScalar()
    Add-Check 'PBT-01' 'Simulated PBT margin probe inserted' ($probeMarginId -gt 0) "marginiId=$probeMarginId ts=$($probeTs.ToString('o'))"
}

$newMission = $null
try {
    $startResp = Unwrap (Invoke-Api -Method POST -Path '/api/mission/start-current' -Headers $h)
    $started = [bool]$startResp.missionStarted
    $openAfter = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionSessions WHERE Completed = 0')
    if ($started -and $openAfter -eq 1) {
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = 'SELECT TOP 1 ID, MissionKey, StartTime, RuntimeMode, Completed FROM dbo.MissionSessions WHERE Completed = 0 ORDER BY ID DESC'
        $r = $cmd.ExecuteReader()
        if ($r.Read()) {
            $newMission = [ordered]@{
                id = [int]$r['ID']; key = [string]$r['MissionKey']
                start = ([datetime]$r['StartTime']).ToString('o')
                runtimeMode = [string]$r['RuntimeMode']; completed = [bool]$r['Completed']
            }
        }
        $r.Close()
    }
    Add-Check 'AST-01' 'New live mission created after reset' ($started -and $openAfter -eq 1 -and $newMission) `
        "missionStarted=$started openAfter=$openAfter" $newMission
    Add-Check 'AST-02' 'New mission key is pbt-* not hist-seq-*' `
        ($newMission -and $newMission.key -like 'pbt-*' -and $newMission.key -notlike 'hist-seq-*') `
        "key=$($newMission.key)"
    Add-Check 'AST-03' 'New mission RuntimeMode=Production' `
        ($newMission -and $newMission.runtimeMode -eq 'Production') "runtimeMode=$($newMission.runtimeMode)"
}
catch {
    Add-Check 'AST-01' 'New live mission created after reset' $false $_.Exception.Message
}

# Guard after reset with new firstPoint
if ($newMission) {
    $fpAfter = Get-Scalar $conn "SELECT MIN(Data) FROM dbo.Margini WHERE Data > '$(Fmt-SqlDate $resetAfter)'"
    if ($fpAfter -and $fpAfter -isnot [DBNull]) {
        $fp = ([datetime]$fpAfter).ToString('yyyy-MM-dd HH:mm:ss')
        $blockHistAfter = [int](Get-Scalar $conn @"
SELECT COUNT(*) FROM dbo.MissionSessions
WHERE StartTime >= '$fp'
  AND (MissionKey IS NULL OR (MissionKey NOT LIKE 'historical-demo-import:%' AND FinalizationReason <> 'HistoricalImport'))
  AND MissionKey LIKE 'hist-seq-%'
"@)
        Add-Check 'GRD-02' 'R9 alreadyTracked excludes hist-seq blockers after autostart' ($blockHistAfter -eq 0) `
            "blockingHistUnderR9=$blockHistAfter firstPoint=$fpAfter"

        $liveBlock = [int](Get-Scalar $conn @"
SELECT COUNT(*) FROM dbo.MissionSessions WHERE StartTime >= '$fp' AND MissionKey LIKE 'pbt-%' AND ID = $($newMission.id)
"@)
        Add-Check 'GRD-03' 'Live pbt mission correctly tracked after firstPoint' ($liveBlock -eq 1) "liveBlock=$liveBlock"
    }
}

# Cleanup: finalize probe mission + remove probe margin (restore 0 open, counts +1 session if we keep finalized)
if ($newMission) {
    try {
        $fin = Unwrap (Invoke-Api POST '/api/mission/finalize-current' $h @{ reason = 'ResetAutostartValidation' })
        Add-Check 'CLN-01' 'Finalize validation mission' ([bool]$fin.missionFinalized) $fin.message $fin
    }
    catch {
        Add-Check 'CLN-01' 'Finalize validation mission' $false $_.Exception.Message
    }
}
if ($probeMarginId) {
    $del = $conn.CreateCommand()
    $del.CommandText = "DELETE FROM dbo.Margini WHERE Id = $probeMarginId"
    [void]$del.ExecuteNonQuery()
    Add-Check 'CLN-02' 'Removed PBT margin probe row' $true "deletedMarginiId=$probeMarginId"
}

$final = [ordered]@{
    sessions = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionSessions')
    samples = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionMarginSamples')
    open = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionSessions WHERE Completed = 0')
    histSeq = [int](Get-Scalar $conn "SELECT COUNT(*) FROM dbo.MissionSessions WHERE MissionKey LIKE 'hist-seq-%'")
}
Add-Check 'POST-01' 'Production hist-seq count unchanged (19)' ($final.histSeq -eq 19) "histSeq=$($final.histSeq)"
Add-Check 'POST-02' 'No open missions after cleanup' ($final.open -eq 0) "open=$($final.open)"
Add-Check 'POST-03' 'Live sessions 1-8 still present' `
    ([int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionSessions WHERE ID BETWEEN 1 AND 8') -eq 8) 'live1-8=8'

$conn.Close()

$report = [ordered]@{
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    baseline = $baseline
    final = $final
    summary = [ordered]@{
        total = $checks.Count
        pass = @($checks | Where-Object { $_.pass }).Count
        fail = @($checks | Where-Object { -not $_.pass }).Count
        allPass = (@($checks | Where-Object { -not $_.pass }).Count -eq 0)
    }
    checks = $checks
}
$outFile = Join-Path $OutDir 'reset_autostart_validation.json'
$report | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $outFile -Encoding UTF8
Write-Host "Report: $outFile"
$checks | ForEach-Object { Write-Host "$($_.status) $($_.id) $($_.name) :: $($_.detail)" }
if (-not $report.summary.allPass) { exit 2 }
