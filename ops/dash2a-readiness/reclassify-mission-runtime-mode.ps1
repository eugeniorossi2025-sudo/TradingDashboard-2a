# Reclassify MissionSessions.RuntimeMode for one session (row only; samples unchanged).
# Requires explicit transaction: pre-checks -> UPDATE -> post-checks -> COMMIT or ROLLBACK.
param(
    [Parameter(Mandatory = $true)]
    [int]$SessionId,
    [ValidateSet('Demo', 'Production')]
    [string]$TargetRuntimeMode = 'Demo',
    [ValidateSet('Production', 'Demo')]
    [string]$ExpectedBeforeMode = 'Production',
    [string]$ConnectionString = '',
    [string]$ProdConfigPath = 'C:\inetpub\wwwroot\shared\appsettings.Production.json',
    [string]$OutDir = '',
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data

function Get-ConnParts([string]$Cs) {
    $parts = @{}
    $Cs -split ';' | ForEach-Object {
        $kv = $_ -split '=', 2
        if ($kv.Count -eq 2) { $parts[$kv[0].Trim()] = $kv[1].Trim() }
    }
    return $parts
}

if (-not $ConnectionString -and (Test-Path -LiteralPath $ProdConfigPath)) {
    $config = Get-Content -LiteralPath $ProdConfigPath -Raw | ConvertFrom-Json
    $ConnectionString = [string]$config.ConnectionStrings.DefaultConnection
}
if (-not $ConnectionString) {
    throw 'ConnectionString required (or ProdConfigPath on VPS backend).'
}

$parts = Get-ConnParts $ConnectionString
if ($parts['Server'] -notlike '*,1434') {
    throw "Refusing: SQL server must be canonical 51.83.159.175,1434. Got Server=$($parts['Server'])"
}
if ($parts['Database'] -ne 'Eugenio-Demo10') {
    throw "Refusing: database must be Eugenio-Demo10. Got Database=$($parts['Database'])"
}

if (-not $OutDir) {
    $OutDir = Join-Path $PSScriptRoot "backups\reclassify_$SessionId`_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
}
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

function Invoke-Scalar($Conn, $Tx, [string]$Sql) {
    $cmd = $Conn.CreateCommand()
    $cmd.Transaction = $Tx
    $cmd.CommandText = $Sql
    return $cmd.ExecuteScalar()
}

function Invoke-NonQuery($Conn, $Tx, [string]$Sql) {
    $cmd = $Conn.CreateCommand()
    $cmd.Transaction = $Tx
    $cmd.CommandText = $Sql
    return $cmd.ExecuteNonQuery()
}

function Export-QueryJson($Conn, $Tx, [string]$Sql, [string]$Path) {
    $cmd = $Conn.CreateCommand()
    $cmd.Transaction = $Tx
    $cmd.CommandText = $Sql
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter $cmd
    $table = New-Object System.Data.DataTable
    [void]$adapter.Fill($table)
    $rows = @()
    foreach ($row in $table.Rows) {
        $obj = [ordered]@{}
        foreach ($col in $table.Columns) {
            $val = $row[$col]
            if ($val -is [DBNull]) { $obj[$col.ColumnName] = $null }
            elseif ($val -is [datetime]) { $obj[$col.ColumnName] = $val.ToString('o') }
            else { $obj[$col.ColumnName] = $val }
        }
        $rows += $obj
    }
    $rows | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $Path -Encoding UTF8
    return $rows.Count
}

$conn = New-Object System.Data.SqlClient.SqlConnection $ConnectionString
$conn.Open()
$stamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$bkpSession = "dbo.MissionSessions_bkp_reclassify_${SessionId}_$stamp"
$bkpSamples = "dbo.MissionMarginSamples_bkp_reclassify_${SessionId}_$stamp"

# --- Phase 1: backups outside transaction (durable rollback) ---
$preSession = Export-QueryJson $conn $null @"
SELECT * FROM dbo.MissionSessions WHERE ID = $SessionId;
"@ (Join-Path $OutDir "MissionSessions_$SessionId`_before.json")

$preSamples = Export-QueryJson $conn $null @"
SELECT * FROM dbo.MissionMarginSamples WHERE SessionId = $SessionId ORDER BY Timestamp, ID;
"@ (Join-Path $OutDir "MissionMarginSamples_$SessionId`_before.json")

if ($preSession -eq 0) {
    $conn.Close()
    throw "MissionSession $SessionId not found."
}

Invoke-NonQuery $conn $null "IF OBJECT_ID(N'$bkpSession', N'U') IS NOT NULL DROP TABLE $bkpSession;"
Invoke-NonQuery $conn $null "SELECT * INTO $bkpSession FROM dbo.MissionSessions WHERE ID = $SessionId;"
Invoke-NonQuery $conn $null "IF OBJECT_ID(N'$bkpSamples', N'U') IS NOT NULL DROP TABLE $bkpSamples;"
Invoke-NonQuery $conn $null "SELECT * INTO $bkpSamples FROM dbo.MissionMarginSamples WHERE SessionId = $SessionId;"

$rollbackSql = @"
-- Rollback RuntimeMode reclassify session $SessionId ($stamp)
BEGIN TRANSACTION;
UPDATE ms
SET ms.RuntimeMode = b.RuntimeMode
FROM dbo.MissionSessions ms
INNER JOIN $bkpSession b ON b.ID = ms.ID
WHERE ms.ID = $SessionId;
-- Expected: @@ROWCOUNT = 1, RuntimeMode restored to '$ExpectedBeforeMode'
-- Samples: unchanged (no UPDATE on MissionMarginSamples)
COMMIT TRANSACTION;
"@
$rollbackSql | Set-Content -LiteralPath (Join-Path $OutDir "rollback_$SessionId.sql") -Encoding UTF8

Write-Host "BACKUP_JSON=$OutDir"
Write-Host "BACKUP_TABLE_SESSION=$bkpSession rows=$preSession"
Write-Host "BACKUP_TABLE_SAMPLES=$bkpSamples rows=$preSamples"
Write-Host "ROLLBACK_SQL=$(Join-Path $OutDir "rollback_$SessionId.sql")"

if ($DryRun) {
    Write-Host "DRY RUN: backups written; no transaction UPDATE."
    $conn.Close()
    exit 0
}

# --- Phase 2: explicit transaction ---
$tx = $conn.BeginTransaction()
try {
    $dbName = [string](Invoke-Scalar $conn $tx 'SELECT DB_NAME();')
    Write-Host "DB_NAME=$dbName"
    if ($dbName -ne 'Eugenio-Demo10') {
        throw "Pre-check failed: DB_NAME=$dbName"
    }

    $pre = Export-QueryJson $conn $tx @"
SELECT ID, MissionKey, RuntimeMode, Completed, StartTime, EndTime, TotalMargin, FinalizationReason
FROM dbo.MissionSessions WHERE ID = $SessionId;
"@ (Join-Path $OutDir "MissionSessions_$SessionId`_pre_tx.json")

    if ($pre -eq 0) { throw "Pre-check: session $SessionId missing inside transaction." }

    $mode = [string](Invoke-Scalar $conn $tx "SELECT RuntimeMode FROM dbo.MissionSessions WHERE ID = $SessionId;")
    $completed = [int](Invoke-Scalar $conn $tx "SELECT CAST(Completed AS int) FROM dbo.MissionSessions WHERE ID = $SessionId;")
    $sampleCountBefore = [int](Invoke-Scalar $conn $tx "SELECT COUNT(*) FROM dbo.MissionMarginSamples WHERE SessionId = $SessionId;")
    $openMissionId = Invoke-Scalar $conn $tx "SELECT TOP (1) ID FROM dbo.MissionSessions WHERE Completed = 0 ORDER BY StartTime DESC;"

    Write-Host "PRE_ID=$SessionId RuntimeMode=$mode Completed=$completed SampleCount=$sampleCountBefore OpenMissionId=$openMissionId"

    if ($mode -ne $ExpectedBeforeMode) {
        throw "Pre-check failed: RuntimeMode='$mode' expected '$ExpectedBeforeMode'."
    }
    if ($completed -ne 1) {
        throw "Pre-check failed: Completed=$completed expected 1."
    }
    if ($SessionId -eq [int]$openMissionId) {
        throw "Pre-check failed: session $SessionId is the open mission; refusing."
    }

    $upd = $conn.CreateCommand()
    $upd.Transaction = $tx
    $upd.CommandText = @"
UPDATE dbo.MissionSessions
SET RuntimeMode = @mode
WHERE ID = @id AND RuntimeMode = @before;
"@
    [void]$upd.Parameters.AddWithValue('@mode', $TargetRuntimeMode)
    [void]$upd.Parameters.AddWithValue('@id', $SessionId)
    [void]$upd.Parameters.AddWithValue('@before', $ExpectedBeforeMode)
    $rowsUpdated = $upd.ExecuteNonQuery()
    Write-Host "UPDATE_ROWCOUNT=$rowsUpdated"

    if ($rowsUpdated -ne 1) {
        throw "UPDATE expected 1 row, got $rowsUpdated."
    }

    $modeAfter = [string](Invoke-Scalar $conn $tx "SELECT RuntimeMode FROM dbo.MissionSessions WHERE ID = $SessionId;")
    $completedAfter = [int](Invoke-Scalar $conn $tx "SELECT CAST(Completed AS int) FROM dbo.MissionSessions WHERE ID = $SessionId;")
    $sampleCountAfter = [int](Invoke-Scalar $conn $tx "SELECT COUNT(*) FROM dbo.MissionMarginSamples WHERE SessionId = $SessionId;")

    Write-Host "POST_RuntimeMode=$modeAfter POST_Completed=$completedAfter POST_SampleCount=$sampleCountAfter"

    if ($modeAfter -ne $TargetRuntimeMode) {
        throw "Post-check failed: RuntimeMode='$modeAfter' expected '$TargetRuntimeMode'."
    }
    if ($completedAfter -ne 1) {
        throw "Post-check failed: Completed changed to $completedAfter."
    }
    if ($sampleCountAfter -ne $sampleCountBefore) {
        throw "Post-check failed: sample count changed $sampleCountBefore -> $sampleCountAfter."
    }

    Export-QueryJson $conn $tx "SELECT * FROM dbo.MissionSessions WHERE ID = $SessionId;" (Join-Path $OutDir "MissionSessions_$SessionId`_after.json") | Out-Null

    $tx.Commit()
    Write-Host "TRANSACTION=COMMITTED"
}
catch {
    if ($tx.Connection -and $tx.Connection.State -eq 'Open') {
        $tx.Rollback()
        Write-Host "TRANSACTION=ROLLBACK"
    }
    $conn.Close()
    throw
}

$conn.Close()

[ordered]@{
    sessionId = $SessionId
    targetRuntimeMode = $TargetRuntimeMode
    backupSessionTable = $bkpSession
    backupSamplesTable = $bkpSamples
    jsonBackupDir = $OutDir
    rollbackSql = (Join-Path $OutDir "rollback_$SessionId.sql")
    committed = $true
} | ConvertTo-Json -Depth 4 | Write-Output
