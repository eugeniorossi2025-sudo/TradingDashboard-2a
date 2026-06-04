# Controlled execution: dry-run -> backup -> apply -> verify (+ auto-rollback on failure).
param(
    [string]$ProdConfigPath = 'C:\inetpub\wwwroot\shared\appsettings.Production.json',
    [string]$ScriptDir = $PSScriptRoot,
    [switch]$DryRunOnly
)

$ErrorActionPreference = 'Stop'

function Get-ConnParts([string]$Cs) {
    $parts = @{}
    $Cs -split ';' | ForEach-Object {
        $kv = $_ -split '=', 2
        if ($kv.Count -eq 2) { $parts[$kv[0].Trim()] = $kv[1].Trim() }
    }
    return $parts
}

function Invoke-SqlFile([string]$Server, [string]$Database, [string]$User, [string]$Password, [string]$FilePath) {
    Write-Host ""
    Write-Host "==> $FilePath"
    $out = sqlcmd -S $Server -d $Database -U $User -P $Password -i $FilePath -W -b 2>&1
    $out | ForEach-Object { Write-Host $_ }
    if ($LASTEXITCODE -ne 0) { throw "sqlcmd failed ($FilePath) exit=$LASTEXITCODE" }
    return ($out | Out-String)
}

function Invoke-SqlQuery([string]$Server, [string]$Database, [string]$User, [string]$Password, [string]$Query) {
    $out = sqlcmd -S $Server -d $Database -U $User -P $Password -Q $Query -W -h-1 -b 2>&1
    if ($LASTEXITCODE -ne 0) { throw "sqlcmd query failed exit=$LASTEXITCODE" }
    return ($out | Out-String)
}

if (-not (Test-Path -LiteralPath $ProdConfigPath)) {
    throw "Shared production config not found: $ProdConfigPath"
}

$config = Get-Content -LiteralPath $ProdConfigPath -Raw | ConvertFrom-Json
$connString = [string]$config.ConnectionStrings.DefaultConnection
if ([string]::IsNullOrWhiteSpace($connString)) { throw 'DefaultConnection missing.' }

$parts = Get-ConnParts $connString
if ($parts['Server'] -notlike '*,1434') {
    throw "Refusing: expected SQL port 1434. Server=$($parts['Server'])"
}
if ($parts['Database'] -ne 'Eugenio-Demo10') {
    throw "Refusing: expected Eugenio-Demo10. Database=$($parts['Database'])"
}

$server = $parts['Server']
$database = $parts['Database']
$user = $parts['User Id']
$password = $parts['Password']

Write-Host "SQL_SERVER=$server"
Write-Host "SQL_DB=$database"
Write-Host "SQL_USER=$user"

# --- Step 1: dry-run ---
$dryOut = Invoke-SqlFile $server $database $user $password (Join-Path $ScriptDir '01-dry-run-overlap-time-recovery.sql')

if ($dryOut -notmatch '\b101\b' -or $dryOut -notmatch '\b104\b') {
    throw 'Dry-run output missing expected session IDs 101-104.'
}

# --- Step 2: backup ---
if ($DryRunOnly) {
    Write-Host 'DryRunOnly: stopping after dry-run.'
    exit 0
}

$backupOut = Invoke-SqlFile $server $database $user $password (Join-Path $ScriptDir '02-backup-mission-sessions-101-104.sql')
if ($backupOut -notmatch 'BACKUP_TABLE=(\S+)') {
    throw 'Could not parse BACKUP_TABLE from backup script output.'
}
$backupTable = $Matches[1]
Write-Host "CONFIRMED_BACKUP_TABLE=$backupTable"

# Snapshot margins/samples/105 before apply
$pre105 = Invoke-SqlQuery $server $database $user $password @"
SET NOCOUNT ON;
SELECT ID, StartTime, EndTime, TotalMargin, RealHandsCount, FinalizationReason, CAST(Completed AS int) AS CompletedInt
FROM dbo.MissionSessions WHERE ID = 105;
"@
Write-Host "PRE_APPLY_SESSION_105:"
Write-Host $pre105

$preMargins = Invoke-SqlQuery $server $database $user $password @"
SET NOCOUNT ON;
SELECT ID, TotalMargin, RealHandsCount FROM dbo.MissionSessions WHERE ID IN (101,102,103,104,105) ORDER BY ID;
"@
Write-Host "PRE_APPLY_MARGINS:"
Write-Host $preMargins

$preSamples = Invoke-SqlQuery $server $database $user $password @"
SET NOCOUNT ON;
SELECT SessionId, COUNT_BIG(*) AS Cnt FROM dbo.MissionMarginSamples WHERE SessionId IN (101,102,103,104,105) GROUP BY SessionId ORDER BY SessionId;
"@
Write-Host "PRE_APPLY_SAMPLES:"
Write-Host $preSamples

# --- Step 3: apply ---
Invoke-SqlFile $server $database $user $password (Join-Path $ScriptDir '03-apply-overlap-time-recovery.sql')

# --- Step 4: verify ---
$verifyOut = Invoke-SqlFile $server $database $user $password (Join-Path $ScriptDir '04-verify-overlap-time-recovery.sql')

$failed = $false
if ($verifyOut -match 'OVERLAP|OverlapSeconds') {
    $overlapRows = Invoke-SqlQuery $server $database $user $password @"
SET NOCOUNT ON;
WITH ordered AS (
    SELECT ID, StartTime, EndTime FROM dbo.MissionSessions WHERE ID IN (101,102,103,104)
)
SELECT COUNT(*) FROM ordered a
INNER JOIN ordered b ON b.ID = a.ID + 1
WHERE a.EndTime > b.StartTime;
"@
    if ([int]($overlapRows.Trim()) -gt 0) {
        Write-Host "VERIFY_FAIL: overlap count > 0"
        $failed = $true
    }
}

$postMargins = Invoke-SqlQuery $server $database $user $password @"
SET NOCOUNT ON;
SELECT ID, TotalMargin, RealHandsCount FROM dbo.MissionSessions WHERE ID IN (101,102,103,104,105) ORDER BY ID;
"@
if ($postMargins -ne $preMargins) {
    Write-Host 'VERIFY_FAIL: TotalMargin or RealHandsCount changed'
    $failed = $true
}

$postSamples = Invoke-SqlQuery $server $database $user $password @"
SET NOCOUNT ON;
SELECT SessionId, COUNT_BIG(*) AS Cnt FROM dbo.MissionMarginSamples WHERE SessionId IN (101,102,103,104,105) GROUP BY SessionId ORDER BY SessionId;
"@
if ($postSamples -ne $preSamples) {
    Write-Host 'VERIFY_FAIL: MissionMarginSamples counts changed'
    $failed = $true
}

$post105 = Invoke-SqlQuery $server $database $user $password @"
SET NOCOUNT ON;
SELECT ID, StartTime, EndTime, TotalMargin, RealHandsCount, FinalizationReason FROM dbo.MissionSessions WHERE ID = 105;
"@
if ($post105 -ne $pre105) {
    Write-Host 'VERIFY_FAIL: session 105 changed'
    $failed = $true
}

$chain = Invoke-SqlQuery $server $database $user $password @"
SET NOCOUNT ON;
SELECT
  s101.EndTime AS End101, s102.StartTime AS Start102, s102.EndTime AS End102, s103.StartTime AS Start103,
  s103.EndTime AS End103, s104.StartTime AS Start104, s104.EndTime AS End104,
  s104.FinalizationReason AS Reason104
FROM dbo.MissionSessions s101
JOIN dbo.MissionSessions s102 ON s102.ID = 102
JOIN dbo.MissionSessions s103 ON s103.ID = 103
JOIN dbo.MissionSessions s104 ON s104.ID = 104
WHERE s101.ID = 101;
"@
Write-Host "POST_CHAIN:"
Write-Host $chain

$openCount = Invoke-SqlQuery $server $database $user $password @"
SET NOCOUNT ON;
SELECT COUNT(*) FROM dbo.MissionSessions WHERE Completed = 0;
"@
Write-Host "OPEN_MISSIONS_COUNT=$($openCount.Trim())"
if ([int]($openCount.Trim()) -gt 1) {
    Write-Host 'VERIFY_FAIL: more than one open mission'
    $failed = $true
}

if ($failed) {
    Write-Host "ROLLBACK: restoring from $backupTable"
    Invoke-SqlQuery $server $database $user $password @"
SET NOCOUNT ON;
BEGIN TRANSACTION;
UPDATE m
SET
  m.StartTime = b.StartTime,
  m.EndTime = b.EndTime,
  m.FinalizationReason = b.FinalizationReason,
  m.ReportPublishedAt = b.ReportPublishedAt,
  m.TotalMargin = b.TotalMargin,
  m.RealHandsCount = b.RealHandsCount,
  m.Completed = b.Completed
FROM dbo.MissionSessions m
INNER JOIN [dbo].[$backupTable] b ON b.ID = m.ID
WHERE m.ID IN (101, 102, 103, 104);
COMMIT TRANSACTION;
SELECT @@ROWCOUNT AS RollbackRows;
"@
    throw 'Verification failed; rollback applied to sessions 101-104.'
}

Write-Host 'SUCCESS: overlap time recovery completed and verified.'
