# Post-import Production validation (read-only + controlled reset/autostart probe).
param(
    [string]$BaseUrl = '',
    [string]$Username = 'admin',
    [string]$Password = 'Admin@123456',
    [string]$ProdConfigPath = 'C:\inetpub\wwwroot\shared\appsettings.Production.json',
    [string]$OutDir = '',
    [switch]$RunResetAutostartProbe
)

if (-not $BaseUrl) {
    $BaseUrl = 'https://localhost'
}
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data

if (-not $OutDir) {
    $OutDir = Join-Path $PSScriptRoot "exports\validation\post_import_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
}
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

$results = New-Object System.Collections.Generic.List[object]

function Add-Check {
    param([string]$Id, [string]$Area, [string]$Name, [bool]$Pass, [string]$Detail, $Evidence = $null)
    $results.Add([ordered]@{
        id      = $Id
        area    = $Area
        name    = $Name
        pass    = $Pass
        status  = if ($Pass) { 'PASS' } else { 'FAIL' }
        detail  = $Detail
        evidence = $Evidence
        atUtc   = (Get-Date).ToUniversalTime().ToString('o')
    }) | Out-Null
}

function Open-Conn([string]$Cs) {
    $c = New-Object System.Data.SqlClient.SqlConnection $Cs
    $c.Open()
    return $c
}

function Get-Scalar($Conn, [string]$Sql) {
    $cmd = $Conn.CreateCommand()
    $cmd.CommandText = $Sql
    return $cmd.ExecuteScalar()
}

function Invoke-Api {
    param([string]$Method, [string]$Path, [hashtable]$Headers = @{}, $Body = $null)
    $uri = "$BaseUrl$Path"
    if ($Body) {
        return Invoke-RestMethod -Uri $uri -Method $Method -Headers $Headers -ContentType 'application/json' -Body ($Body | ConvertTo-Json) -TimeoutSec 90
    }
    return Invoke-RestMethod -Uri $uri -Method $Method -Headers $Headers -TimeoutSec 90
}

function Unwrap-Api($Obj) {
    if ($null -eq $Obj) { return $null }
    if ($Obj.PSObject.Properties['data']) { return $Obj.data }
    return $Obj
}

# --- DB ---
$config = Get-Content -LiteralPath $ProdConfigPath -Raw | ConvertFrom-Json
$connString = [string]$config.ConnectionStrings.DefaultConnection
$conn = Open-Conn $connString

$dbSessions = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionSessions')
$dbSamples = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionMarginSamples')
$dbOpen = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionSessions WHERE Completed = 0')
$dbHist = [int](Get-Scalar $conn "SELECT COUNT(*) FROM dbo.MissionSessions WHERE MissionKey LIKE 'hist-seq-%'")
$dbLive = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionSessions WHERE ID BETWEEN 1 AND 8')

$liveKeys = @()
$cmd = $conn.CreateCommand()
$cmd.CommandText = 'SELECT ID, MissionKey, TotalMargin FROM dbo.MissionSessions WHERE ID BETWEEN 1 AND 8 ORDER BY ID'
$r = $cmd.ExecuteReader()
while ($r.Read()) {
    $liveKeys += [ordered]@{ id = [int]$r['ID']; key = [string]$r['MissionKey']; totalMargin = [decimal]$r['TotalMargin'] }
}
$r.Close()

Add-Check -Id 'DB-01' -Area 'DB' -Name 'Session count 27' -Pass ($dbSessions -eq 27) -Detail "sessions=$dbSessions" -Evidence @{ expected = 27; actual = $dbSessions }
Add-Check -Id 'DB-02' -Area 'DB' -Name 'Sample count 48856' -Pass ($dbSamples -eq 48856) -Detail "samples=$dbSamples" -Evidence @{ expected = 48856; actual = $dbSamples }
Add-Check -Id 'DB-03' -Area 'DB' -Name 'No open missions' -Pass ($dbOpen -eq 0) -Detail "open=$dbOpen" -Evidence @{ open = $dbOpen }
Add-Check -Id 'DB-04' -Area 'DB' -Name 'Live sessions 1-8 intact' -Pass ($dbLive -eq 8) -Detail "liveRows=$dbLive" -Evidence $liveKeys
Add-Check -Id 'DB-05' -Area 'DB' -Name 'Imported hist-seq count 19' -Pass ($dbHist -eq 19) -Detail "histSeq=$dbHist" -Evidence @{ histSeq = $dbHist }

# R9 guard simulation
$resetAtObj = Get-Scalar $conn "SELECT TOP 1 Value FROM dbo.Configurations WHERE [K] = 'MISSION_LAST_RESET_AT_UTC'"
$resetAt = if ($resetAtObj -is [DBNull] -or -not $resetAtObj) { $null } else { [datetime]$resetAtObj }
$firstPointObj = $null
if ($resetAt) {
    $firstPointObj = Get-Scalar $conn "SELECT MIN(Data) FROM dbo.Margini WHERE Data > '$($resetAt.ToString('yyyy-MM-dd HH:mm:ss'))'"
}
$firstPoint = if ($firstPointObj -is [DBNull] -or -not $firstPointObj) { $null } else { [datetime]$firstPointObj }

$blockingHist = 0
$blockingHistR9 = 0
$blockingAny = 0
if ($firstPoint) {
    $fp = $firstPoint.ToString('yyyy-MM-dd HH:mm:ss')
    $blockingHist = [int](Get-Scalar $conn @"
SELECT COUNT(*) FROM dbo.MissionSessions
WHERE StartTime >= '$fp' AND MissionKey LIKE 'hist-seq-%'
"@)
    $blockingHistR9 = [int](Get-Scalar $conn @"
SELECT COUNT(*) FROM dbo.MissionSessions
WHERE StartTime >= '$fp'
  AND (MissionKey IS NULL
    OR (MissionKey NOT LIKE 'historical-demo-import:%' AND FinalizationReason <> 'HistoricalImport'))
  AND MissionKey LIKE 'hist-seq-%'
"@)
    $blockingAny = [int](Get-Scalar $conn @"
SELECT COUNT(*) FROM dbo.MissionSessions
WHERE StartTime >= '$fp'
  AND (MissionKey IS NULL
    OR (MissionKey NOT LIKE 'historical-demo-import:%' AND FinalizationReason <> 'HistoricalImport'))
"@)
}

Add-Check -Id 'DB-06' -Area 'DB' -Name 'hist-seq do not block alreadyTracked (timestamp)' -Pass ($blockingHist -eq 0) `
    -Detail "firstPoint=$firstPoint blockingHist=$blockingHist" `
    -Evidence @{ resetAt = $(if ($resetAt) { $resetAt.ToString('o') } else { $null }); firstPoint = $(if ($firstPoint) { $firstPoint.ToString('o') } else { $null }); blockingHist = $blockingHist }

Add-Check -Id 'DB-07' -Area 'DB' -Name 'R9 guard would not count hist-seq as blockers' -Pass ($blockingHistR9 -eq 0) `
    -Detail "blockingHistUnderR9=$blockingHistR9 (note: OneByOneHistoricalImport not in R9 exclude list)" `
    -Evidence @{ blockingHistR9 = $blockingHistR9; blockingAny = $blockingAny }

# --- API ---
$token = $null
try {
    $login = Invoke-Api -Method POST -Path '/api/Auth/login' -Body @{ username = $Username; password = $Password }
    $loginData = Unwrap-Api $login
    $token = if ($loginData.token) { $loginData.token } elseif ($loginData.Token) { $loginData.Token } else { $null }
    Add-Check -Id 'API-01' -Area 'API' -Name 'Login' -Pass ([bool]$token) -Detail "tokenLength=$($token.Length)" -Evidence @{ baseUrl = $BaseUrl }
}
catch {
    Add-Check -Id 'API-01' -Area 'API' -Name 'Login' -Pass $false -Detail $_.Exception.Message -Evidence @{ baseUrl = $BaseUrl }
}

if ($token) {
    $h = @{ Authorization = "Bearer $token" }

    try {
        $idx2025 = Unwrap-Api (Invoke-Api -Method GET -Path '/api/mission/reports/index?runtimeMode=Production&fromUtc=2025-01-01&toUtc=2025-12-31&skip=0&limit=200' -Headers $h)
        Add-Check -Id 'API-02' -Area 'API' -Name 'Index Production 2025 returns 19 imported sessions' -Pass ([int]$idx2025.total -eq 19) `
            -Detail "total2025=$($idx2025.total)" `
            -Evidence @{ total = [int]$idx2025.total; sessionIds = @(@($idx2025.items) | ForEach-Object { $_.sessionId }) }
    }
    catch {
        Add-Check -Id 'API-02' -Area 'API' -Name 'Index Production 2025 returns 19 imported sessions' -Pass $false -Detail $_.Exception.Message
    }

    try {
        $idxAll = Unwrap-Api (Invoke-Api -Method GET -Path '/api/mission/reports/index?runtimeMode=Production&fromUtc=2025-01-01&toUtc=2026-12-31&skip=0&limit=200' -Headers $h)
        Add-Check -Id 'API-03' -Area 'API' -Name 'Index Production 2025-2026 returns 27 sessions' -Pass ([int]$idxAll.total -eq 27) `
            -Detail "total=$($idxAll.total)" `
            -Evidence @{ total = [int]$idxAll.total; liveIds = @(@($idxAll.items) | Where-Object { $_.sessionId -le 8 } | ForEach-Object { $_.sessionId }) }
    }
    catch {
        Add-Check -Id 'API-03' -Area 'API' -Name 'Index Production 2025-2026 returns 27 sessions' -Pass $false -Detail $_.Exception.Message
    }

    try {
        $range2025 = Unwrap-Api (Invoke-Api -Method GET -Path '/api/mission/report/range?runtimeMode=Production&from=2025-01-01&to=2025-12-31&format=json&summary=true' -Headers $h)
        $rangeSamples = [int]$range2025.sampleCount
        $importedSampleSum = [int](Get-Scalar $conn "SELECT COUNT(*) FROM dbo.MissionMarginSamples s INNER JOIN dbo.MissionSessions m ON m.ID = s.SessionId WHERE m.MissionKey LIKE 'hist-seq-%'")
        Add-Check -Id 'API-04' -Area 'API' -Name 'Range Production 2025 sampleCount matches imported samples' -Pass ($rangeSamples -eq $importedSampleSum) `
            -Detail "rangeSamples=$rangeSamples importedSamples=$importedSampleSum" `
            -Evidence @{ rangeSamples = $rangeSamples; importedSampleSum = $importedSampleSum; totals = $range2025.totals }
    }
    catch {
        Add-Check -Id 'API-04' -Area 'API' -Name 'Range Production 2025 sampleCount matches imported samples' -Pass $false -Detail $_.Exception.Message
    }

    try {
        $sessionChecks = @(10, 16, 28)
        $sessionEvidence = @()
        $sessionPass = $true
        foreach ($sid in $sessionChecks) {
            try {
                $rep = Unwrap-Api (Invoke-Api -Method GET -Path "/api/mission/report/$sid?format=json&summary=false" -Headers $h)
                $sc = if ($rep.samples) { @($rep.samples).Count } elseif ($rep.totals.sampleCount) { [int]$rep.totals.sampleCount } else { 0 }
                $dbSc = [int](Get-Scalar $conn "SELECT COUNT(*) FROM dbo.MissionMarginSamples WHERE SessionId = $sid")
                $ok = ($sc -eq $dbSc -and $sc -gt 0)
                if (-not $ok) { $sessionPass = $false }
                $sessionEvidence += [ordered]@{ sessionId = $sid; apiSamples = $sc; dbSamples = $dbSc; ok = $ok }
            }
            catch {
                $sessionPass = $false
                $sessionEvidence += [ordered]@{ sessionId = $sid; error = $_.Exception.Message }
            }
        }
        Add-Check -Id 'API-05' -Area 'API' -Name 'Individual session reports load (10,16,28)' -Pass $sessionPass `
            -Detail 'Compared API sample counts vs DB per session' -Evidence $sessionEvidence
    }
    catch {
        Add-Check -Id 'API-05' -Area 'API' -Name 'Individual session reports load (10,16,28)' -Pass $false -Detail $_.Exception.Message
    }

    try {
        $current = Unwrap-Api (Invoke-Api -Method GET -Path '/api/mission/current' -Headers $h)
        Add-Check -Id 'API-06' -Area 'API' -Name 'No open mission in /mission/current' -Pass (-not $current.hasOpenMission) `
            -Detail "hasOpenMission=$($current.hasOpenMission)" -Evidence $current
    }
    catch {
        Add-Check -Id 'API-06' -Area 'API' -Name 'No open mission in /mission/current' -Pass $false -Detail $_.Exception.Message
    }
}

# --- Reset / autostart probe (optional, side effects) ---
if ($RunResetAutostartProbe -and $token) {
    $h = @{ Authorization = "Bearer $token" }
    $sessionsBefore = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionSessions')
    $openBefore = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionSessions WHERE Completed = 0')

    if ($openBefore -gt 0) {
        Add-Check -Id 'RST-01' -Area 'Reset' -Name 'Reset probe skipped' -Pass $false -Detail "Refusing reset probe: $openBefore open mission(s)"
    }
    else {
        try {
            $resetResp = Invoke-Api -Method POST -Path '/api/decider/reset' -Headers $h
            $resetData = Unwrap-Api $resetResp
            Add-Check -Id 'RST-01' -Area 'Reset' -Name 'Dashboard reset API' -Pass $true -Detail 'POST /api/decider/reset OK' -Evidence $resetData

            Start-Sleep -Seconds 2
            $resetAtAfter = Get-Scalar $conn "SELECT TOP 1 Value FROM dbo.Configurations WHERE [K] = 'MISSION_LAST_RESET_AT_UTC'"
            Add-Check -Id 'RST-02' -Area 'Reset' -Name 'Reset boundary recorded' -Pass ($null -ne $resetAtAfter -and $resetAtAfter -isnot [DBNull]) `
                -Detail "MISSION_LAST_RESET_AT=$resetAtAfter" -Evidence @{ value = [string]$resetAtAfter }

            $sessionsAfter = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionSessions')
            Add-Check -Id 'RST-03' -Area 'Reset' -Name 'Session count unchanged after reset' -Pass ($sessionsAfter -eq $sessionsBefore) `
                -Detail "before=$sessionsBefore after=$sessionsAfter"

            try {
                $startResp = Invoke-Api -Method POST -Path '/api/mission/start-current' -Headers $h
                $startData = Unwrap-Api $startResp
                $openAfter = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionSessions WHERE Completed = 0')
                $started = [bool]$startData.missionStarted
                Add-Check -Id 'RST-04' -Area 'AutoStart' -Name 'start-current after reset' -Pass $true `
                    -Detail "missionStarted=$started openAfter=$openAfter message=$($startData.message)" -Evidence $startData

                if ($started -and $openAfter -eq 1) {
                    $newOpen = $conn.CreateCommand()
                    $newOpen.CommandText = 'SELECT TOP 1 ID, MissionKey, StartTime FROM dbo.MissionSessions WHERE Completed = 0 ORDER BY ID DESC'
                    $nr = $newOpen.ExecuteReader()
                    $newMission = $null
                    if ($nr.Read()) {
                        $newMission = [ordered]@{
                            id = [int]$nr['ID']; key = [string]$nr['MissionKey']; start = ([datetime]$nr['StartTime']).ToString('o')
                        }
                    }
                    $nr.Close()
                    $isHist = $newMission -and [string]$newMission.key -like 'hist-seq-*'
                    Add-Check -Id 'RST-05' -Area 'AutoStart' -Name 'New live mission is not hist-seq import' -Pass (-not $isHist) `
                        -Detail "newMission=$($newMission | ConvertTo-Json -Compress)" -Evidence $newMission
                }
                else {
                    Add-Check -Id 'RST-05' -Area 'AutoStart' -Name 'New live mission created (requires trading data after reset)' -Pass $false `
                        -Detail 'No new mission yet — expected if no Margini point after reset boundary. Guard simulation DB-06/07 still validates hist-seq non-interference.' `
                        -Evidence @{ missionStarted = $started; openAfter = $openAfter; note = 'Resume trading to complete live autostart proof' }
                }
            }
            catch {
                Add-Check -Id 'RST-04' -Area 'AutoStart' -Name 'start-current after reset' -Pass $false -Detail $_.Exception.Message
            }
        }
        catch {
            Add-Check -Id 'RST-01' -Area 'Reset' -Name 'Dashboard reset API' -Pass $false -Detail $_.Exception.Message
        }
    }
}

$conn.Close()

$report = [ordered]@{
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    baseUrl        = $BaseUrl
    runResetProbe  = [bool]$RunResetAutostartProbe.IsPresent
    summary        = [ordered]@{
        total  = $results.Count
        pass   = @($results | Where-Object { $_.pass }).Count
        fail   = @($results | Where-Object { -not $_.pass }).Count
        allPass = (@($results | Where-Object { -not $_.pass }).Count -eq 0)
    }
    checks = $results
}

$outFile = Join-Path $OutDir 'post_import_validation.json'
$report | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $outFile -Encoding UTF8
Write-Host "Validation report: $outFile"
Write-Host "PASS=$($report.summary.pass) FAIL=$($report.summary.fail) ALL_PASS=$($report.summary.allPass)"
$results | ForEach-Object { Write-Host "$($_.status) $($_.id) $($_.name) :: $($_.detail)" }

if (-not $report.summary.allPass) { exit 2 }
