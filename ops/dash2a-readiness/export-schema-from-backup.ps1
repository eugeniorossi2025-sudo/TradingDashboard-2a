# Read-only: export schema for the 30 pre-mission tables.
# Primary source: live Eugenio-Demo10 on SQLEXPRESS01 (same schema as pre-mission .bak).
# Optional: restore .bak to side DB when Windows/sysadmin auth is available.
param(
    [string]$BackupPath = 'C:\Program Files\Microsoft SQL Server\MSSQL17.SQLEXPRESS01\MSSQL\Backup\Eugenio-Demo10_pre_mission_tables_20260524_192426.bak',
    [string]$Server = '.\SQLEXPRESS01',
    [string]$Database = 'Eugenio-Demo10',
    [string]$SqlUser = 'sa3',
    [string]$SqlPassword = 'LionGest123@',
    [string]$OutputPath = 'ops/dash2a-readiness/production-30-tables-from-backup.txt'
)

$ErrorActionPreference = 'Stop'

$MissionTables = @(
    'MissionSessions',
    'MissionMarginSamples',
    'UserNotificationSettings',
    'UserAccessEvents'
)

function Invoke-Sql([string]$Query, [string]$Db = $Database) {
    $result = sqlcmd -S $Server -U $SqlUser -P $SqlPassword -d $Db -Q $Query -W -s '|' -h-1 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw ($result -join "`n")
    }
    return @($result)
}

Write-Output "SERVER=$Server"
Write-Output "DATABASE=$Database"
Write-Output "BACKUP_REF=$BackupPath"

if (Test-Path $BackupPath) {
    Write-Output "BACKUP_EXISTS=YES"
} else {
    Write-Output "BACKUP_EXISTS=NO (using live DB schema export instead)"
}

$excludeList = ($MissionTables | ForEach-Object { "N'$_'" }) -join ', '
$tableCount = (Invoke-Sql "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME NOT IN ($excludeList);")[0]
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
WHERE t.TABLE_TYPE = 'BASE TABLE'
  AND t.TABLE_SCHEMA = 'dbo'
  AND t.TABLE_NAME NOT IN ($excludeList)
ORDER BY t.TABLE_NAME, c.ORDINAL_POSITION;
"@

$lines = Invoke-Sql $schemaSql
$tableNames = Invoke-Sql @"
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA='dbo' AND TABLE_NAME NOT IN ($excludeList)
ORDER BY TABLE_NAME;
"@

$header = @(
    '# DASH2A schema for 30 pre-mission tables',
    "# Source: $Database on $Server",
    "# Backup reference: $BackupPath",
    "# Excluded mission tables: $($MissionTables -join ', ')",
    "# Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')",
    "# TableCount: $tableCount",
    '#',
    '# TABLE LIST:',
    ($tableNames | ForEach-Object { "#   $_" }),
    '#',
    '# Format: TABLE|ORD|COLUMN|TYPE|SIZE|NULLABLE|KEY',
    ''
)

$outDir = Split-Path -Parent $OutputPath
if ($outDir -and -not (Test-Path $outDir)) {
    New-Item -ItemType Directory -Path $outDir -Force | Out-Null
}

($header + $lines) | Set-Content -Path $OutputPath -Encoding utf8
Write-Output "OUTPUT=$OutputPath"
Write-Output "EXPORT_DONE=YES"
