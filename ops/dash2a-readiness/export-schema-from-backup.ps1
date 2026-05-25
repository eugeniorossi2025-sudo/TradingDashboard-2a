# Read-only: restore pre-mission backup to a side database and export column schema.
# Does NOT modify Eugenio-Demo10 production database.
param(
    [string]$BackupPath = 'C:\Program Files\Microsoft SQL Server\MSSQL17.SQLEXPRESS01\MSSQL\Backup\Eugenio-Demo10_pre_mission_tables_20260524_192426.bak',
    [string]$Server = '.\SQLEXPRESS01',
    [string]$SideDb = 'Dash2A_SchemaRef_PreMission',
    [string]$OutputPath = 'ops/dash2a-readiness/production-30-tables-from-backup.txt'
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $BackupPath)) {
    throw "Backup not found: $BackupPath"
}

function Invoke-Sql([string]$Query, [string]$Database = 'master') {
    $result = sqlcmd -S $Server -E -d $Database -Q $Query -W -s '|' -h-1 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw ($result -join "`n")
    }
    return $result
}

Write-Output "BACKUP=$BackupPath"
Write-Output "SERVER=$Server"
Write-Output "SIDE_DB=$SideDb"

$fileList = sqlcmd -S $Server -E -Q "RESTORE FILELISTONLY FROM DISK = N'$BackupPath';" -W -s '|' -h-1
if ($LASTEXITCODE -ne 0) { throw "RESTORE FILELISTONLY failed" }

$dataLogical = ($fileList | Select-Object -Skip 2 | Select-Object -First 1).Split('|')[0].Trim()
$logLogical = ($fileList | Select-Object -Skip 3 | Select-Object -First 1).Split('|')[0].Trim()
if (-not $dataLogical -or -not $logLogical) {
    throw "Could not parse logical file names from backup"
}

$dataDir = Invoke-Sql "SELECT CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS nvarchar(4000));" | Select-Object -First 1
$logDir = Invoke-Sql "SELECT CAST(SERVERPROPERTY('InstanceDefaultLogPath') AS nvarchar(4000));" | Select-Object -First 1
$dataPath = Join-Path $dataDir.Trim() "$SideDb.mdf"
$logPath = Join-Path $logDir.Trim() "${SideDb}_log.ldf"

Invoke-Sql @"
IF DB_ID(N'$SideDb') IS NOT NULL
BEGIN
  ALTER DATABASE [$SideDb] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
  DROP DATABASE [$SideDb];
END
"@ | Out-Null

$restoreSql = @"
RESTORE DATABASE [$SideDb]
FROM DISK = N'$BackupPath'
WITH
  MOVE N'$dataLogical' TO N'$dataPath',
  MOVE N'$logLogical' TO N'$logPath',
  REPLACE,
  STATS = 10;
"@
Invoke-Sql $restoreSql | Out-Null

$tableCount = Invoke-Sql "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE';" $SideDb | Select-Object -First 1
Write-Output "TABLE_COUNT=$tableCount"

$schemaSql = @"
SELECT
  t.TABLE_NAME,
  c.ORDINAL_POSITION,
  c.COLUMN_NAME,
  c.DATA_TYPE,
  COALESCE(CAST(c.CHARACTER_MAXIMUM_LENGTH AS varchar(20)), CAST(c.NUMERIC_PRECISION AS varchar(20)), '') AS SIZE,
  c.IS_NULLABLE,
  CASE WHEN pk.COLUMN_NAME IS NOT NULL THEN 'PK' ELSE '' END AS KEY_TYPE
FROM INFORMATION_SCHEMA.TABLES t
JOIN INFORMATION_SCHEMA.COLUMNS c
  ON c.TABLE_SCHEMA = t.TABLE_SCHEMA AND c.TABLE_NAME = t.TABLE_NAME
LEFT JOIN (
  SELECT ku.TABLE_NAME, ku.COLUMN_NAME
  FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
  JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku
    ON tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
  WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
) pk ON pk.TABLE_NAME = c.TABLE_NAME AND pk.COLUMN_NAME = c.COLUMN_NAME
WHERE t.TABLE_TYPE = 'BASE TABLE' AND t.TABLE_SCHEMA = 'dbo'
ORDER BY t.TABLE_NAME, c.ORDINAL_POSITION;
"@

$lines = Invoke-Sql $schemaSql $SideDb
$header = @(
    "# DASH2A production schema from pre-mission backup",
    "# Backup: $BackupPath",
    "# Side DB: $SideDb on $Server",
    "# Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')",
    "# TableCount: $tableCount",
    "# Format: TABLE|ORD|COLUMN|TYPE|SIZE|NULLABLE|KEY",
    ""
)

$outDir = Split-Path -Parent $OutputPath
if ($outDir -and -not (Test-Path $outDir)) {
    New-Item -ItemType Directory -Path $outDir -Force | Out-Null
}

($header + $lines) | Set-Content -Path $OutputPath -Encoding utf8
Write-Output "OUTPUT=$OutputPath"
Write-Output "EXPORT_DONE=YES"
