# Collaudo Control Room override - SOLO VPS Decisore (runner DASH2A-DECISORE o RDP).
# DB runtime: 51.83.159.175,1434 (firewall solo da 51.178.16.37). NON usare 1433/LocalDB.
# Decide: http://127.0.0.1/api/proactive/decide (IIS app pool Proactive).
#
# Uso:
#   powershell -ExecutionPolicy Bypass -File .\tools\control-room-vps-collaudo.ps1
#
# Env:
#   COLLAUDO_CONNECTION_STRING  default C:\Decisore\appsettings.json
#   DECISORE_URL                default http://127.0.0.1
#   DECIDE_USERNAME / DECIDE_PASSWORD

$ErrorActionPreference = 'Stop'

function Ok($name, $detail) {
    if ($detail) { Write-Host "OK   $name - $detail" -ForegroundColor Green }
    else { Write-Host "OK   $name" -ForegroundColor Green }
}
function Fail($name, $detail) {
    if ($detail) { Write-Host "FAIL $name - $detail" -ForegroundColor Red }
    else { Write-Host "FAIL $name" -ForegroundColor Red }
    Write-Host "`nVERDICT: FAIL - STOP DEPLOY" -ForegroundColor Red
    exit 1
}
function Log($line) { Write-Host "LOG  $line" }

function Invoke-Decide {
    param([string]$Computer)
    $qs = @{
        USERNAME = $env:DECIDE_USERNAME
        PASSWORD = $env:DECIDE_PASSWORD
        COMPUTER = $Computer
        TAVOLO = '1'
        SALDO_INIZIALE = '1000'
        MARGINE = '0'
        COLPO_MARTINGALA = '0'
        MAZZO = '5'
        PBT = 'P'
        VALORE_GIOCATO = '10'
        STATO = 'ATTESA'
    }
    $query = ($qs.GetEnumerator() | ForEach-Object { "{0}={1}" -f $_.Key, [uri]::EscapeDataString([string]$_.Value) }) -join '&'
    $url = "$($env:DECISORE_URL.TrimEnd('/'))/api/proactive/decide?$query"
    $res = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 60
    $text = $res.Content.Trim()
    if ($text -notmatch '^\d+$') { throw "decide non numerico: $text" }
    return [int]$text
}

function Open-Sql($cs) {
    $c = New-Object System.Data.SqlClient.SqlConnection($cs)
    $c.Open()
    return $c
}

function Ensure-OverrideTable($conn) {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = @"
IF OBJECT_ID(N'[dbo].[ControlRoomCommandOverrides]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ControlRoomCommandOverrides](
        [PC] NVARCHAR(50) NOT NULL CONSTRAINT [PK_ControlRoomCommandOverrides] PRIMARY KEY,
        [ActionCode] INT NOT NULL,
        [CommandType] NVARCHAR(32) NOT NULL,
        [CreatedAtUtc] DATETIME2 NOT NULL CONSTRAINT [DF_ControlRoomCommandOverrides_CreatedAtUtc] DEFAULT(SYSUTCDATETIME()),
        [CreatedByUserId] INT NULL
    );
END;
"@
    $cmd.ExecuteNonQuery() | Out-Null
}

function Set-Override($conn, [string]$pc, [int]$ac, [string]$ct) {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = @"
IF EXISTS (SELECT 1 FROM dbo.ControlRoomCommandOverrides WHERE PC = @pc)
    UPDATE dbo.ControlRoomCommandOverrides SET ActionCode = @ac, CommandType = @ct, CreatedAtUtc = SYSUTCDATETIME() WHERE PC = @pc;
ELSE
    INSERT INTO dbo.ControlRoomCommandOverrides (PC, ActionCode, CommandType, CreatedAtUtc) VALUES (@pc, @ac, @ct, SYSUTCDATETIME());
"@
    $null = $cmd.Parameters.AddWithValue('@pc', $pc)
    $null = $cmd.Parameters.AddWithValue('@ac', $ac)
    $null = $cmd.Parameters.AddWithValue('@ct', $ct)
    $cmd.ExecuteNonQuery() | Out-Null
}

function Get-OverrideCount($conn, [string]$pc) {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = 'SELECT COUNT(1) FROM dbo.ControlRoomCommandOverrides WHERE PC = @pc'
    $null = $cmd.Parameters.AddWithValue('@pc', $pc)
    return [int]$cmd.ExecuteScalar()
}

function Clear-Override($conn, [string]$pc) {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = 'DELETE FROM dbo.ControlRoomCommandOverrides WHERE PC = @pc'
    $null = $cmd.Parameters.AddWithValue('@pc', $pc)
    $cmd.ExecuteNonQuery() | Out-Null
}

function Get-LatestOverrideLog($conn, [string]$pc) {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = @"
SELECT TOP 1 Description FROM dbo.ApiLogs
WHERE Category = @pc AND Description LIKE '%CONTROL_ROOM_OVERRIDE%'
ORDER BY CreatedAt DESC
"@
    $null = $cmd.Parameters.AddWithValue('@pc', $pc)
    return [string]$cmd.ExecuteScalar()
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$liveSettings = 'C:\Decisore\appsettings.json'
if (-not $env:COLLAUDO_CONNECTION_STRING) {
    if (Test-Path -LiteralPath $liveSettings) {
        $json = Get-Content -LiteralPath $liveSettings -Raw | ConvertFrom-Json
        $env:COLLAUDO_CONNECTION_STRING = $json.ConnectionStrings.DefaultConnection
    }
    elseif (Test-Path (Join-Path $repoRoot 'decision-engine\Decisore\appsettings.json')) {
        Write-Warning 'Uso appsettings repo - su VPS usare C:\Decisore\appsettings.json live'
        $json = Get-Content (Join-Path $repoRoot 'decision-engine\Decisore\appsettings.json') -Raw | ConvertFrom-Json
        $env:COLLAUDO_CONNECTION_STRING = $json.ConnectionStrings.DefaultConnection
    }
}

if (-not $env:DECISORE_URL) { $env:DECISORE_URL = 'http://127.0.0.1' }
if (-not $env:DECIDE_USERNAME) { $env:DECIDE_USERNAME = 'eugenio' }
if (-not $env:DECIDE_PASSWORD) { $env:DECIDE_PASSWORD = '123456' }

if ([string]::IsNullOrWhiteSpace($env:COLLAUDO_CONNECTION_STRING)) {
    Fail 'connection string' 'Impostare COLLAUDO_CONNECTION_STRING o C:\Decisore\appsettings.json'
}

if ($env:COLLAUDO_CONNECTION_STRING -notmatch ',1434') {
    Fail 'connection string port' 'Runtime Decisore deve usare 51.83.159.175,1434 (vedi DASH2A-INFRASTRUCTURE.md). 1433 e LocalDB non validi.'
}

$pc1 = 'CR_E2E_PC1'
$pc2 = 'CR_E2E_PC2'
$pc3 = 'CR_E2E_PC3'
$pc4 = 'CR_E2E_PC4'
$all = @($pc1, $pc2, $pc3, $pc4)

Write-Host "`n=== 1. DB + CONTINUA (AC0) ===" -ForegroundColor Cyan
$conn = Open-Sql $env:COLLAUDO_CONNECTION_STRING
try {
    Ok 'SQL connect' ($env:COLLAUDO_CONNECTION_STRING -replace 'Password=[^;]+', 'Password=***')
    Ensure-OverrideTable $conn
    foreach ($pc in $all) { Clear-Override $conn $pc }

    $baseline = Invoke-Decide $pc2
    if ($baseline -eq 9) { Fail 'baseline decide' 'response=9 (engine/config)' }
    Ok 'baseline decide PC2' "action=$baseline"
    Log "DECIDE baseline pc=$pc2 response=$baseline"

    Set-Override $conn $pc2 0 'Continue'
    if ((Get-OverrideCount $conn $pc2) -ne 1) { Fail 'AC0 queued' 'pending count' }

    $d1 = Invoke-Decide $pc2
    if ($d1 -ne 0) { Fail 'CONTINUA first decide' "expected 0 got $d1" }
    Ok 'CONTINUA first decide' 'overrideAction=0'

    if ((Get-OverrideCount $conn $pc2) -ne 0) { Fail 'CONTINUA DB consume' 'override still pending' }
    Ok 'CONTINUA DB consumed' 'count=0'

    $logLine = Get-LatestOverrideLog $conn $pc2
    if (-not $logLine -or $logLine -notmatch 'CONTROL_ROOM_OVERRIDE' -or $logLine -notmatch 'overrideAction=0' -or $logLine -notmatch 'consumed=true') {
        $logDetail = if ($logLine) { $logLine } else { 'missing' }
        Fail 'CONTINUA ApiLogs' $logDetail
    }
    Ok 'CONTINUA log' ($logLine.Split("`n") | Where-Object { $_ -match 'CONTROL_ROOM_OVERRIDE' } | Select-Object -First 1)

    $d2 = Invoke-Decide $pc2
    if ($d2 -eq 9) { Fail 'CONTINUA second decide' 'response=9' }
    Ok 'CONTINUA second decide' "action=$d2 (engine normal)"

    Write-Host "`n=== 2. AZZERA (AC2) ===" -ForegroundColor Cyan
    Set-Override $conn $pc2 2 'ResetMartingale'
    $d3 = Invoke-Decide $pc2
    if ($d3 -ne 2) { Fail 'AZZERA first decide' "expected 2 got $d3" }
    Ok 'AZZERA first decide' 'overrideAction=2'
    if ((Get-OverrideCount $conn $pc2) -ne 0) { Fail 'AZZERA DB consume' 'override still pending' }

    $d4 = Invoke-Decide $pc2
    if ($d4 -eq 9) { Fail 'AZZERA second decide' 'response=9' }
    Ok 'AZZERA second decide' "action=$d4 (engine normal)"

    Write-Host "`n=== 3. Isolamento PC ===" -ForegroundColor Cyan
    Set-Override $conn $pc2 0 'Continue'
    $o1 = Invoke-Decide $pc1
    $o3 = Invoke-Decide $pc3
    if ((Get-OverrideCount $conn $pc2) -ne 1) { Fail 'isolation AC0' "PC1=$o1 PC3=$o3 pending lost" }
    Ok 'isolation AC0 PC2' "PC1=$o1 PC3=$o3 PC2 pending=1"

    Clear-Override $conn $pc2
    Set-Override $conn $pc2 2 'ResetMartingale'
    $o4 = Invoke-Decide $pc4
    if ((Get-OverrideCount $conn $pc2) -ne 1) { Fail 'isolation AC2' "PC4=$o4 pending lost" }
    Ok 'isolation AC2 PC2' "PC4=$o4 PC2 pending=1"

    foreach ($pc in $all) { Clear-Override $conn $pc }
}
finally {
    $conn.Close()
    $conn.Dispose()
}

Write-Host "`nVERDICT: PASS - collaudo VPS Decisore OK. WebApi/UI non ancora collaudati." -ForegroundColor Green
exit 0
