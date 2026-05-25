# Clone production-like schema locally and preserve mission/auth data from source DB.
# Does NOT touch production. Source DB is left unchanged.
param(
    [string]$Server = '(localdb)\MSSQLLocalDB',
    [string]$SourceDb = 'Dash2A_LocalImportTest',
    [string]$TargetDb = 'Dash2A_LocalProdLike',
    [switch]$Force,
    [switch]$RegenerateDdl,
    [switch]$CopyOnly
)

$ErrorActionPreference = 'Stop'
$Root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Set-Location $Root

$DdlPath = 'ops/dash2a-readiness/production-30-tables-ddl.sql'
$MissionDdlPath = 'ops/dash2a-readiness/create-missing-mission-tables.sql'
$GeneratorPath = 'ops/dash2a-readiness/generate-production-30-tables-ddl.ps1'

function Invoke-Sql([string]$Query, [string]$Database = 'master') {
    $batch = "SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON; $Query"
    $out = sqlcmd -S $Server -d $Database -Q $batch -W -h-1 -b 2>&1
    if ($LASTEXITCODE -ne 0) { throw ($out -join "`n") }
    $text = ($out | ForEach-Object { "$_" }) -join "`n"
    if ($text -match '^(Messaggio|Msg) ') { throw $text }
    return @($out)
}

function Get-Scalar([string]$Query, [string]$Database) {
    $line = (Invoke-Sql $Query $Database | Where-Object { $_ -and $_ -notmatch 'righe interessate|rows affected' } | Select-Object -First 1)
    return $line.ToString().Trim()
}

function Get-TableColumns([string]$Database, [string]$Table) {
    $rows = Invoke-Sql @"
SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME=N'$Table'
ORDER BY ORDINAL_POSITION;
"@ $Database
    return @($rows | ForEach-Object { $_.ToString().Trim() } | Where-Object { $_ -match '^[A-Za-z_][A-Za-z0-9_]*$' })
}

function Test-Database([string]$Name) {
    $v = Get-Scalar "SELECT COUNT(*) FROM sys.databases WHERE name = N'$Name';" 'master'
    return [int]$v -gt 0
}

function Test-Table([string]$Db, [string]$Table) {
    $v = Get-Scalar "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME=N'$Table';" $Db
    return [int]$v -gt 0
}

function Test-HasIdentity([string]$Db, [string]$Table) {
    $v = Get-Scalar "SELECT COUNT(*) FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'dbo.$Table');" $Db
    return [int]$v -gt 0
}

function Copy-Table {
    param(
        [string]$Table
    )
    if (-not (Test-Table $SourceDb $Table)) {
        Write-Output "SKIP $Table (missing in source)"
        return
    }
    if (-not (Test-Table $TargetDb $Table)) {
        Write-Output "SKIP $Table (missing in target)"
        return
    }

    $sourceCount = [int](Get-Scalar "SELECT COUNT(*) FROM [dbo].[$Table];" $SourceDb)
    if ($sourceCount -eq 0) {
        Write-Output "SKIP $Table (0 rows in source)"
        return
    }

    $cols = Get-TableColumns $TargetDb $Table
    $sourceCols = Get-TableColumns $SourceDb $Table
    $shared = @($cols | Where-Object { $sourceCols -contains $_ })
    if ($shared.Count -eq 0) {
        throw "No shared columns for table $Table"
    }
    $colList = ($shared | ForEach-Object { "[$_]" }) -join ', '

    $useIdentity = Test-HasIdentity $TargetDb $Table
    $batch = @(
        'SET QUOTED_IDENTIFIER ON;',
        'SET ANSI_NULLS ON;',
        "DELETE FROM [dbo].[$Table];"
    )
    if ($useIdentity) { $batch += "SET IDENTITY_INSERT [dbo].[$Table] ON;" }
    $batch += "INSERT INTO [dbo].[$Table] ($colList) SELECT $colList FROM [$SourceDb].[dbo].[$Table];"
    if ($useIdentity) { $batch += "SET IDENTITY_INSERT [dbo].[$Table] OFF;" }

    $tempFile = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), "dash2a-copy-$Table-$([Guid]::NewGuid().ToString('N')).sql")
    ($batch -join "`r`n") | Set-Content -Path $tempFile -Encoding UTF8
    try {
        $out = sqlcmd -S $Server -d $TargetDb -i $tempFile -b 2>&1
        if ($LASTEXITCODE -ne 0) { throw ($out -join "`n") }
    } finally {
        Remove-Item $tempFile -Force -ErrorAction SilentlyContinue
    }

    $targetCount = [int](Get-Scalar "SELECT COUNT(*) FROM [dbo].[$Table];" $TargetDb)
    Write-Output "COPY $Table : $sourceCount -> $targetCount"
}

Write-Output "=== DASH2A local prod-like clone ==="
Write-Output "Server=$Server"
Write-Output "Source=$SourceDb"
Write-Output "Target=$TargetDb"

if (-not (Test-Database $SourceDb)) {
    throw "Source database not found: $SourceDb"
}

if ((Test-Database $TargetDb) -and -not $Force -and -not $CopyOnly) {
    throw "Target database '$TargetDb' already exists. Re-run with -Force to recreate it or -CopyOnly to refresh data."
}

if (-not $CopyOnly) {
if ($RegenerateDdl -or -not (Test-Path $DdlPath)) {
    & $GeneratorPath
}

if (Test-Database $TargetDb) {
    Write-Output "Dropping existing target database..."
    Invoke-Sql @"
ALTER DATABASE [$TargetDb] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE [$TargetDb];
"@ 'master' | Out-Null
}

Write-Output "Creating target database..."
Invoke-Sql "CREATE DATABASE [$TargetDb];" 'master' | Out-Null

Write-Output "Applying production 30-table DDL..."
sqlcmd -S $Server -d $TargetDb -i $DdlPath -b | Out-Null
if ($LASTEXITCODE -ne 0) { throw "Failed applying $DdlPath" }

Write-Output "Applying mission table DDL..."
sqlcmd -S $Server -d $TargetDb -i $MissionDdlPath | Out-Null
if ($LASTEXITCODE -ne 0) { throw "Failed applying $MissionDdlPath" }

$table30 = [int](Get-Scalar "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME NOT IN ('MissionSessions','MissionMarginSamples','UserNotificationSettings','UserAccessEvents');" $TargetDb)
Write-Output "Target shell tables (excl. mission): $table30"
}

if (-not (Test-Database $TargetDb)) {
    throw "Target database '$TargetDb' not found. Run without -CopyOnly first."
}

Write-Output "Copying preserved local data..."
Copy-Table -Table 'AspNetRoles'
Copy-Table -Table 'AspNetRoleClaims'
Copy-Table -Table 'Users_v2'
Copy-Table -Table 'AspNetUserRoles'
Copy-Table -Table 'AspNetUserClaims'
Copy-Table -Table 'AspNetUserLogins'
Copy-Table -Table 'MissionSessions'
Copy-Table -Table 'MissionMarginSamples'
Copy-Table -Table 'UserNotificationSettings'
Copy-Table -Table 'UserAccessEvents'

Write-Output ""
Write-Output "=== VERIFY ==="
$verify = @(
    'MissionSessions',
    'MissionMarginSamples',
    'UserAccessEvents',
    'UserNotificationSettings',
    'Users_v2',
    'Pc_CurrentStatus',
    'Configurations'
)
foreach ($t in $verify) {
    if (Test-Table $TargetDb $t) {
        $c = Get-Scalar "SELECT COUNT(*) FROM [dbo].[$t];" $TargetDb
        Write-Output ("  {0,-28} {1}" -f $t, $c)
    } else {
        Write-Output ("  {0,-28} MISSING" -f $t)
    }
}

$conn = "Server=$Server;Database=$TargetDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"
Write-Output ""
Write-Output "DONE. Source DB '$SourceDb' unchanged."
Write-Output "Use this connection string for local dev:"
Write-Output $conn
