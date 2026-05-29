# Controlled import: local mission export -> production MissionSessions/MissionMarginSamples ONLY.
# Run on VPS backend (1434 local). NEVER restore full DB. NEVER UPDATE/DELETE prod rows.
param(
    [Parameter(Mandatory = $true)]
    [string]$MetaFile,
    [switch]$DryRun,
    [switch]$SkipBackupRecommendation,
    [int]$SampleBatchSize = 5000,
    [string]$ProdConfigPath = 'C:\inetpub\wwwroot\shared\appsettings.Production.json'
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data

if (-not (Test-Path -LiteralPath $MetaFile)) {
    throw "Meta file not found: $MetaFile"
}

$exportDir = Split-Path -Parent $MetaFile
$meta = Get-Content -LiteralPath $MetaFile -Raw | ConvertFrom-Json
$sessionFile = Join-Path $exportDir $meta.sessionFile
$sampleFile = Join-Path $exportDir $meta.sampleFile

if (-not (Test-Path -LiteralPath $sessionFile)) { throw "Session file not found: $sessionFile" }
if (-not (Test-Path -LiteralPath $sampleFile)) { throw "Sample file not found: $sampleFile" }

if (-not $SkipBackupRecommendation -and -not $DryRun) {
    Write-Host 'ATTENZIONE: eseguire backup DB produzione PRIMA dell import reale.'
    Write-Host '  BACKUP DATABASE [Eugenio-Demo10] TO DISK = ... WITH INIT;'
    throw 'Ripetire con -SkipBackupRecommendation solo dopo backup verificato.'
}

if (-not (Test-Path -LiteralPath $ProdConfigPath)) {
    throw "Production config not found: $ProdConfigPath"
}

$config = Get-Content -LiteralPath $ProdConfigPath -Raw | ConvertFrom-Json
$connString = [string]$config.ConnectionStrings.DefaultConnection
if ([string]::IsNullOrWhiteSpace($connString)) {
    throw 'DefaultConnection missing from production config.'
}

$parts = @{}
$connString -split ';' | ForEach-Object {
    $kv = $_ -split '=', 2
    if ($kv.Count -eq 2) { $parts[$kv[0].Trim()] = $kv[1].Trim() }
}
if ($parts['Server'] -notlike '*,1434') {
    throw "Refusing import: expected SQL on port 1434, got $($parts['Server'])"
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

$sessions = Get-Content -LiteralPath $sessionFile -Raw | ConvertFrom-Json
if ($sessions -isnot [array]) { $sessions = @($sessions) }

$report = [ordered]@{
    startedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    dryRun = [bool]$DryRun
    exportMeta = $meta
    prodBefore = @{}
    prodAfter = @{}
    insertedSessions = @()
    skippedSessions = @()
    errors = @()
    sampleRowsInserted = 0
}

$conn = Open-Conn $connString
$report.prodBefore.sessions = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionSessions')
$report.prodBefore.samples = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionMarginSamples')

$existingKeys = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)
$keyCmd = $conn.CreateCommand()
$keyCmd.CommandText = 'SELECT MissionKey FROM dbo.MissionSessions WHERE MissionKey IS NOT NULL'
$keyReader = $keyCmd.ExecuteReader()
while ($keyReader.Read()) {
    [void]$existingKeys.Add([string]$keyReader[0])
}
$keyReader.Close()

Write-Host "Prod before: sessions=$($report.prodBefore.sessions) samples=$($report.prodBefore.samples) existingKeys=$($existingKeys.Count)"
Write-Host "Export: sessions=$($sessions.Count) samples=$($meta.sampleCount) dryRun=$DryRun"

$sessionIdMap = @{} # sourceId -> new prod Id

Write-Host "Indexing samples by sourceSessionId..."
$samplesBySession = @{}
$lineNum = 0
foreach ($line in [System.IO.File]::ReadLines($sampleFile)) {
    $lineNum++
    if ([string]::IsNullOrWhiteSpace($line)) { continue }
    $sample = $line | ConvertFrom-Json
    $sid = [int]$sample.sourceSessionId
    if (-not $samplesBySession.ContainsKey($sid)) {
        $samplesBySession[$sid] = New-Object System.Collections.Generic.List[object]
    }
    [void]$samplesBySession[$sid].Add($sample)
    if ($lineNum % 50000 -eq 0) { Write-Host "  indexed lines: $lineNum" }
}
Write-Host "Indexed $($samplesBySession.Keys.Count) sessions with samples"

foreach ($s in $sessions) {
    $key = [string]$s.missionKey
    if ($key -and $existingKeys.Contains($key)) {
        $report.skippedSessions += [ordered]@{
            sourceId = $s.sourceId
            missionKey = $key
            reason = 'MissionKey already exists in production'
        }
        continue
    }

    if ($DryRun) {
        $sampleCount = if ($samplesBySession.ContainsKey([int]$s.sourceId)) { $samplesBySession[[int]$s.sourceId].Count } else { 0 }
        $report.insertedSessions += [ordered]@{
            sourceId = $s.sourceId
            missionKey = $key
            runtimeMode = $s.runtimeMode
            startTime = $s.startTime
            endTime = $s.endTime
            samples = $sampleCount
            action = 'would_insert'
        }
        continue
    }

    $tx = $conn.BeginTransaction()
    try {
        $insert = $conn.CreateCommand()
        $insert.Transaction = $tx
        $insert.CommandText = @'
INSERT INTO dbo.MissionSessions (
    MissionKey, StartTime, EndTime, TotalMargin, RealHandsCount,
    LastTotalMarginForRealHands, GlobalTarget, ActiveTables, KFactor,
    RuntimeMode, Completed, ReportPublishedAt, FinalizationReason, CreatedAt
)
OUTPUT INSERTED.ID
VALUES (
    @MissionKey, @StartTime, @EndTime, @TotalMargin, @RealHandsCount,
    @LastTotalMarginForRealHands, @GlobalTarget, @ActiveTables, @KFactor,
    @RuntimeMode, @Completed, @ReportPublishedAt, @FinalizationReason, @CreatedAt
);
'@
        [void]$insert.Parameters.AddWithValue('@MissionKey', $(if ($key) { $key } else { [DBNull]::Value }))
        [void]$insert.Parameters.AddWithValue('@StartTime', [datetime]::Parse($s.startTime))
        [void]$insert.Parameters.AddWithValue('@EndTime', $(if ($s.endTime) { [datetime]::Parse($s.endTime) } else { [DBNull]::Value }))
        [void]$insert.Parameters.AddWithValue('@TotalMargin', [decimal]$s.totalMargin)
        [void]$insert.Parameters.AddWithValue('@RealHandsCount', [int]$s.realHandsCount)
        [void]$insert.Parameters.AddWithValue('@LastTotalMarginForRealHands', $(if ($null -ne $s.lastTotalMarginForRealHands) { [decimal]$s.lastTotalMarginForRealHands } else { [DBNull]::Value }))
        [void]$insert.Parameters.AddWithValue('@GlobalTarget', [decimal]$s.globalTarget)
        [void]$insert.Parameters.AddWithValue('@ActiveTables', [int]$s.activeTables)
        [void]$insert.Parameters.AddWithValue('@KFactor', [decimal]$s.kFactor)
        [void]$insert.Parameters.AddWithValue('@RuntimeMode', [string]$s.runtimeMode)
        [void]$insert.Parameters.AddWithValue('@Completed', [bool]$s.completed)
        [void]$insert.Parameters.AddWithValue('@ReportPublishedAt', $(if ($s.reportPublishedAt) { [datetime]::Parse($s.reportPublishedAt) } else { [DBNull]::Value }))
        [void]$insert.Parameters.AddWithValue('@FinalizationReason', $(if ($s.finalizationReason) { [string]$s.finalizationReason } else { [DBNull]::Value }))
        [void]$insert.Parameters.AddWithValue('@CreatedAt', [datetime]::Parse($s.createdAt))

        $newId = [int]$insert.ExecuteScalar()
        $sessionIdMap[[int]$s.sourceId] = $newId
        if ($key) { [void]$existingKeys.Add($key) }

        $batch = New-Object System.Data.DataTable
        [void]$batch.Columns.Add('SessionId', [int])
        [void]$batch.Columns.Add('Timestamp', [datetime])
        [void]$batch.Columns.Add('TotalMargin', [decimal])
        [void]$batch.Columns.Add('ActiveTables', [int])
        [void]$batch.Columns.Add('VmCurrent', [decimal])
        [void]$batch.Columns.Add('RuntimeMode', [string])

        $sampleRowsForSession = 0
        $sessionSamples = $samplesBySession[[int]$s.sourceId]
        if ($sessionSamples) {
            foreach ($sample in $sessionSamples) {
            $row = $batch.NewRow()
            $row['SessionId'] = $newId
            $row['Timestamp'] = [datetime]::Parse($sample.timestamp)
            $row['TotalMargin'] = [decimal]$sample.totalMargin
            $row['ActiveTables'] = [int]$sample.activeTables
            $row['VmCurrent'] = [decimal]$sample.vmCurrent
            $row['RuntimeMode'] = [string]$sample.runtimeMode
            [void]$batch.Rows.Add($row)
            $sampleRowsForSession++

            if ($batch.Rows.Count -ge $SampleBatchSize) {
                $bulk = New-Object System.Data.SqlClient.SqlBulkCopy($conn, [System.Data.SqlClient.SqlBulkCopyOptions]::Default, $tx)
                $bulk.DestinationTableName = 'dbo.MissionMarginSamples'
                $bulk.BatchSize = $SampleBatchSize
                $bulk.WriteToServer($batch)
                $report.sampleRowsInserted += $batch.Rows.Count
                $batch.Rows.Clear()
            }
            }
        }

        if ($batch.Rows.Count -gt 0) {
            $bulk = New-Object System.Data.SqlClient.SqlBulkCopy($conn, [System.Data.SqlClient.SqlBulkCopyOptions]::Default, $tx)
            $bulk.DestinationTableName = 'dbo.MissionMarginSamples'
            $bulk.BatchSize = $SampleBatchSize
            $bulk.WriteToServer($batch)
            $report.sampleRowsInserted += $batch.Rows.Count
        }

        $tx.Commit()
        $report.insertedSessions += [ordered]@{
            sourceId = $s.sourceId
            newProdId = $newId
            missionKey = $key
            samplesInserted = $sampleRowsForSession
            action = 'inserted'
        }
        Write-Host "Inserted session sourceId=$($s.sourceId) -> prodId=$newId key=$key samples=$sampleRowsForSession"
    }
    catch {
        $tx.Rollback()
        $report.errors += [ordered]@{
            sourceId = $s.sourceId
            missionKey = $key
            error = $_.Exception.Message
        }
        Write-Warning "FAILED session sourceId=$($s.sourceId): $($_.Exception.Message)"
    }
}

$report.prodAfter.sessions = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionSessions')
$report.prodAfter.samples = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionMarginSamples')
$conn.Close()

$report.completedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
$reportFile = Join-Path $exportDir ("import_report_{0}.json" -f (Get-Date -Format 'yyyyMMdd_HHmmss'))
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportFile -Encoding UTF8

Write-Host ""
Write-Host "=== IMPORT REPORT ==="
Write-Host "DryRun: $DryRun"
Write-Host "Inserted sessions: $($report.insertedSessions.Count)"
Write-Host "Skipped sessions:  $($report.skippedSessions.Count)"
Write-Host "Sample rows inserted: $($report.sampleRowsInserted)"
Write-Host "Errors: $($report.errors.Count)"
Write-Host "Prod sessions: $($report.prodBefore.sessions) -> $($report.prodAfter.sessions)"
Write-Host "Prod samples:  $($report.prodBefore.samples) -> $($report.prodAfter.samples)"
Write-Host "Report file: $reportFile"

if ($report.errors.Count -gt 0) { exit 2 }
