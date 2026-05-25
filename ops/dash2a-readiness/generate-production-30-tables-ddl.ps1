# Generates idempotent CREATE TABLE script from production-30-tables-from-backup.txt
param(
    [string]$SchemaPath = 'ops/dash2a-readiness/production-30-tables-from-backup.txt',
    [string]$OutputPath = 'ops/dash2a-readiness/production-30-tables-ddl.sql'
)

$ErrorActionPreference = 'Stop'

function Get-SqlType([string]$DataType, [string]$Size) {
    switch ($DataType) {
        'nvarchar' { if ($Size -eq '-1') { return 'NVARCHAR(MAX)' }; return "NVARCHAR($Size)" }
        'varchar' { if ($Size -eq '-1') { return 'VARCHAR(MAX)' }; return "VARCHAR($Size)" }
        'varbinary' { if ($Size -eq '-1') { return 'VARBINARY(MAX)' }; return "VARBINARY($Size)" }
        'int' { return 'INT' }
        'bigint' { return 'BIGINT' }
        'bit' { return 'BIT' }
        'datetime' { return 'DATETIME' }
        'datetime2' { return 'DATETIME2' }
        'datetimeoffset' { return 'DATETIMEOFFSET' }
        'numeric' { return "NUMERIC($Size,0)" }
        'decimal' { return "DECIMAL($Size,0)" }
        default { throw "Unsupported type: $DataType" }
    }
}

$tables = @{}
Get-Content $SchemaPath | ForEach-Object {
    if ($_ -match '^\s*#' -or $_ -match '^\s*$' -or $_ -match 'rows affected') { return }
    $parts = $_ -split '\|'
    if ($parts.Count -lt 7) { return }
    $table = $parts[0].Trim()
    $tables[$table] += ,@{
        Ordinal = [int]$parts[1]
        Name = $parts[2]
        DataType = $parts[3]
        Size = $parts[4]
        Nullable = $parts[5]
        KeyType = $parts[6]
    }
}

$lines = @(
    '/*',
    '  DASH2A — production 30-table shell (pre-mission backup schema).',
    '  Generated from production-30-tables-from-backup.txt',
    '  IF NOT EXISTS only — no DROP, no INSERT.',
    '*/',
    'SET XACT_ABORT ON;',
    'GO',
    ''
)

foreach ($tableName in ($tables.Keys | Sort-Object)) {
    $cols = $tables[$tableName] | Sort-Object Ordinal
    $pkCols = @($cols | Where-Object { $_.KeyType -eq 'PK' } | ForEach-Object { $_.Name })
    $colDefs = @()
    foreach ($c in $cols) {
        $sqlType = Get-SqlType $c.DataType $c.Size
        $nullSql = if ($c.Nullable -eq 'NO') { 'NOT NULL' } else { 'NULL' }
        $colDefs += "        [$($c.Name)] $sqlType $nullSql"
    }
    $pkSql = if ($pkCols.Count -gt 0) {
        $pkName = "PK_$tableName"
        ",`n        CONSTRAINT [$pkName] PRIMARY KEY ($(([string]::Join(', ', ($pkCols | ForEach-Object { "[$_]" })))))"
    } else { '' }

    $lines += @(
        "IF OBJECT_ID(N'[dbo].[$tableName]', N'U') IS NULL",
        'BEGIN',
        "    CREATE TABLE [dbo].[$tableName] (",
        ($colDefs -join ",`n"),
        "        $pkSql".TrimEnd(','),
        '    );',
        'END;',
        'GO',
        ''
    )
}

$outDir = Split-Path -Parent $OutputPath
if ($outDir -and -not (Test-Path $outDir)) {
    New-Item -ItemType Directory -Path $outDir -Force | Out-Null
}

$lines | Set-Content -Path $OutputPath -Encoding utf8
Write-Output "Wrote $($tables.Count) tables to $OutputPath"
