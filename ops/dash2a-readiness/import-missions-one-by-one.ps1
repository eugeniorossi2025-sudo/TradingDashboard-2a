# One-by-one mission import: local DB or export JSON -> production (INSERT only).
# Each mission: own transaction, new identity, optional regenerated dates/keys, rollback SQL artifact.
param(
    [ValidateSet('LocalDb', 'Export')]
    [string]$Source = 'Export',
    [string]$LocalServer = '(localdb)\MSSQLLocalDB',
    [string]$LocalDb = 'Dash2A_LocalProdLike',
    [string]$MetaFile = '',
    [ValidateSet('Production', 'All')]
    [string]$RuntimeModeFilter = 'Production',
    [ValidateSet('DryRun', 'Apply')]
    [string]$Mode = 'DryRun',
    [switch]$RegenerateDates,
    [switch]$OneByOne,
    [datetime]$SequenceStartDate = ([datetime]'2025-01-01T08:00:00Z'),
    [int]$SequenceGapMinutes = 1440,
    [int]$MaxMissions = 0,
    [int]$SampleBatchSize = 5000,
    [string]$ProdConfigPath = 'C:\inetpub\wwwroot\shared\appsettings.Production.json',
    [string]$OutDir = ''
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data

$doRegenerateDates = if ($PSBoundParameters.ContainsKey('RegenerateDates')) { $RegenerateDates.IsPresent } else { $true }
$doOneByOne = if ($PSBoundParameters.ContainsKey('OneByOne')) { $OneByOne.IsPresent } else { $true }

$scriptDir = $PSScriptRoot
if (-not $OutDir) {
    $OutDir = Join-Path $scriptDir 'exports\one-by-one'
}
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

$stamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$runId = "onebyone_$stamp"
$logDir = Join-Path $OutDir $runId
New-Item -ItemType Directory -Force -Path $logDir | Out-Null
$rollbackDir = Join-Path $logDir 'rollback'
New-Item -ItemType Directory -Force -Path $rollbackDir | Out-Null

function Open-Conn([string]$Cs) {
    $c = New-Object System.Data.SqlClient.SqlConnection $Cs
    $c.Open()
    return $c
}

function Get-Scalar($Conn, [string]$Sql, $Transaction = $null) {
    $cmd = $Conn.CreateCommand()
    $cmd.CommandText = $Sql
    if ($Transaction) { $cmd.Transaction = $Transaction }
    return $cmd.ExecuteScalar()
}

function Get-ProdConnString([string]$ConfigPath) {
    if (-not (Test-Path -LiteralPath $ConfigPath)) {
        throw "Production config not found: $ConfigPath"
    }
    $config = Get-Content -LiteralPath $ConfigPath -Raw | ConvertFrom-Json
    $cs = [string]$config.ConnectionStrings.DefaultConnection
    if ([string]::IsNullOrWhiteSpace($cs)) { throw 'DefaultConnection missing.' }
    $parts = @{}
    $cs -split ';' | ForEach-Object {
        $kv = $_ -split '=', 2
        if ($kv.Count -eq 2) { $parts[$kv[0].Trim()] = $kv[1].Trim() }
    }
    if ($parts['Server'] -notlike '*,1434') {
        throw "Refusing prod write: expected SQL port 1434, got $($parts['Server'])"
    }
    return $cs
}

function Load-SessionsFromLocalDb {
    param([string]$Server, [string]$Db, [string]$ModeFilter)
    $where = if ($ModeFilter -eq 'All') { '1=1' } else { "RuntimeMode = 'Production'" }
    $cs = "Server=$Server;Database=$Db;Trusted_Connection=True;TrustServerCertificate=True;"
    $conn = Open-Conn $cs
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = @"
SELECT ID, MissionKey, StartTime, EndTime, TotalMargin, RealHandsCount,
       LastTotalMarginForRealHands, GlobalTarget, ActiveTables, KFactor,
       RuntimeMode, Completed, ReportPublishedAt, FinalizationReason, CreatedAt
FROM dbo.MissionSessions
WHERE $where
ORDER BY StartTime, ID;
"@
    $reader = $cmd.ExecuteReader()
    $rows = @()
    while ($reader.Read()) {
        $rows += [ordered]@{
            sourceId = [int]$reader['ID']
            missionKey = if ($reader['MissionKey'] -is [DBNull]) { $null } else { [string]$reader['MissionKey'] }
            startTime = ([datetime]$reader['StartTime']).ToString('o')
            endTime = if ($reader['EndTime'] -is [DBNull]) { $null } else { ([datetime]$reader['EndTime']).ToString('o') }
            totalMargin = [decimal]$reader['TotalMargin']
            realHandsCount = [int]$reader['RealHandsCount']
            lastTotalMarginForRealHands = if ($reader['LastTotalMarginForRealHands'] -is [DBNull]) { $null } else { [decimal]$reader['LastTotalMarginForRealHands'] }
            globalTarget = [decimal]$reader['GlobalTarget']
            activeTables = [int]$reader['ActiveTables']
            kFactor = [decimal]$reader['KFactor']
            runtimeMode = [string]$reader['RuntimeMode']
            completed = [bool]$reader['Completed']
            reportPublishedAt = if ($reader['ReportPublishedAt'] -is [DBNull]) { $null } else { ([datetime]$reader['ReportPublishedAt']).ToString('o') }
            finalizationReason = if ($reader['FinalizationReason'] -is [DBNull]) { $null } else { [string]$reader['FinalizationReason'] }
            createdAt = ([datetime]$reader['CreatedAt']).ToString('o')
        }
    }
    $reader.Close()
    $conn.Close()
    return $rows
}

function Load-SamplesFromLocalDb {
    param([string]$Server, [string]$Db, [int[]]$SessionIds)
    if ($SessionIds.Count -eq 0) { return @{} }
    $cs = "Server=$Server;Database=$Db;Trusted_Connection=True;TrustServerCertificate=True;"
    $conn = Open-Conn $cs
    $idList = ($SessionIds -join ',')
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = @"
SELECT SessionId, Timestamp, TotalMargin, ActiveTables, VmCurrent, RuntimeMode
FROM dbo.MissionMarginSamples
WHERE SessionId IN ($idList)
ORDER BY SessionId, Timestamp, ID;
"@
    $cmd.CommandTimeout = 600
    $reader = $cmd.ExecuteReader()
    $map = @{}
    while ($reader.Read()) {
        $sid = [int]$reader['SessionId']
        if (-not $map.ContainsKey($sid)) {
            $map[$sid] = New-Object System.Collections.Generic.List[object]
        }
        [void]$map[$sid].Add([ordered]@{
            sourceSessionId = $sid
            timestamp = ([datetime]$reader['Timestamp']).ToString('o')
            totalMargin = [decimal]$reader['TotalMargin']
            activeTables = [int]$reader['ActiveTables']
            vmCurrent = [decimal]$reader['VmCurrent']
            runtimeMode = [string]$reader['RuntimeMode']
        })
    }
    $reader.Close()
    $conn.Close()
    return $map
}

function Load-SessionsFromExport {
    param([string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) { throw "Meta file not found: $Path" }
    $meta = Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
    $exportDir = Split-Path -Parent $Path
    $sessionFile = Join-Path $exportDir $meta.sessionFile
    $sessions = Get-Content -LiteralPath $sessionFile -Raw | ConvertFrom-Json
    if ($sessions -isnot [array]) { $sessions = @($sessions) }
    if ($RuntimeModeFilter -eq 'Production') {
        $sessions = @($sessions | Where-Object { $_.runtimeMode -eq 'Production' })
    }
    $sessions = @($sessions | Sort-Object { [datetime]$_.startTime }, { [int]$_.sourceId })
    return @{ meta = $meta; exportDir = $exportDir; sessions = $sessions }
}

function Load-SamplesFromExport {
    param([string]$ExportDir, [object]$Meta)
    $sampleFile = Join-Path $ExportDir $Meta.sampleFile
    $map = @{}
    foreach ($line in [System.IO.File]::ReadLines($sampleFile)) {
        if ([string]::IsNullOrWhiteSpace($line)) { continue }
        $sample = $line | ConvertFrom-Json
        $sid = [int]$sample.sourceSessionId
        if (-not $map.ContainsKey($sid)) {
            $map[$sid] = New-Object System.Collections.Generic.List[object]
        }
        [void]$map[$sid].Add($sample)
    }
    return $map
}

function Build-RegeneratedPlan {
    param([array]$Sessions, [datetime]$StartDate, [int]$GapMinutes, [bool]$RegenDates)
    $plan = @()
    $seq = 0
    foreach ($s in $Sessions) {
        $seq++
        $oldStart = [datetime]::Parse($s.startTime)
        $oldEnd = if ($s.endTime) { [datetime]::Parse($s.endTime) } else { $null }
        $duration = if ($oldEnd) { $oldEnd - $oldStart } else { [TimeSpan]::FromHours(8) }

        if ($RegenDates) {
            $newStart = $StartDate.AddMinutes(($seq - 1) * $GapMinutes)
            $newEnd = $newStart.Add($duration)
            $newKey = ('hist-seq-{0:D4}-{1:yyyyMMddHHmmss}' -f $seq, $newStart)
        }
        else {
            $newStart = $oldStart
            $newEnd = $oldEnd
            $newKey = $s.missionKey
        }

        $plan += [ordered]@{
            sequence = $seq
            sourceId = [int]$s.sourceId
            sourceMissionKey = $s.missionKey
            newMissionKey = $newKey
            newStartTime = $newStart.ToString('o')
            newEndTime = if ($newEnd) { $newEnd.ToString('o') } else { $null }
            durationMinutes = [math]::Round($duration.TotalMinutes, 2)
            runtimeMode = 'Production'
            completed = $true
            finalizationReason = 'OneByOneHistoricalImport'
            totalMargin = [decimal]$s.totalMargin
            realHandsCount = [int]$s.realHandsCount
            lastTotalMarginForRealHands = $s.lastTotalMarginForRealHands
            globalTarget = [decimal]$s.globalTarget
            activeTables = [int]$s.activeTables
            kFactor = [decimal]$s.kFactor
            sourceStartTime = $s.startTime
            sourceEndTime = $s.endTime
        }
    }
    return $plan
}

function Shift-SampleTimestamps {
    param([object]$Samples, [datetime]$OldStart, [datetime]$NewStart)
    if (-not $Samples -or $Samples.Count -eq 0) { return @() }
    $offset = $NewStart - $OldStart
    $shifted = @()
    foreach ($sample in $Samples) {
        $ts = [datetime]::Parse($sample.timestamp)
        $shifted += [ordered]@{
            timestamp = $ts.Add($offset).ToString('o')
            totalMargin = [decimal]$sample.totalMargin
            activeTables = [int]$sample.activeTables
            vmCurrent = [decimal]$sample.vmCurrent
            runtimeMode = 'Production'
        }
    }
    return $shifted
}

function New-SampleBulkCopy {
    param($Conn, $Tx, [int]$BatchSize)
    $bulk = New-Object System.Data.SqlClient.SqlBulkCopy($Conn, [System.Data.SqlClient.SqlBulkCopyOptions]::Default, $Tx)
    $bulk.DestinationTableName = 'dbo.MissionMarginSamples'
    $bulk.BatchSize = $BatchSize
    [void]$bulk.ColumnMappings.Add('SessionId', 'SessionId')
    [void]$bulk.ColumnMappings.Add('Timestamp', 'Timestamp')
    [void]$bulk.ColumnMappings.Add('TotalMargin', 'TotalMargin')
    [void]$bulk.ColumnMappings.Add('ActiveTables', 'ActiveTables')
    [void]$bulk.ColumnMappings.Add('VmCurrent', 'VmCurrent')
    [void]$bulk.ColumnMappings.Add('RuntimeMode', 'RuntimeMode')
    return $bulk
}

function Write-RollbackSql {
    param([string]$Path, [int]$ProdSessionId, [string]$MissionKey, [int]$SampleCount)
    $generatedAt = (Get-Date).ToUniversalTime().ToString('o')
    $content = @"
-- Rollback for one-by-one import (DELETE only rows created by import script)
-- MissionSessionId: $ProdSessionId
-- MissionKey: $MissionKey
-- SamplesExpected: $SampleCount
-- Generated: $generatedAt

BEGIN TRANSACTION;
DELETE FROM dbo.MissionMarginSamples WHERE SessionId = $ProdSessionId;
DELETE FROM dbo.MissionSessions WHERE ID = $ProdSessionId;
-- Verify: SELECT COUNT(*) FROM dbo.MissionSessions WHERE ID = $ProdSessionId; -- expect 0
COMMIT TRANSACTION;
"@
    Set-Content -LiteralPath $Path -Value $content -Encoding UTF8
}

# --- Load source data ---
$exportMeta = $null
$exportDir = $OutDir
if ($Source -eq 'LocalDb') {
    Write-Host "Loading from LocalDb $LocalServer / $LocalDb filter=$RuntimeModeFilter"
    $sessions = Load-SessionsFromLocalDb -Server $LocalServer -Db $LocalDb -ModeFilter $RuntimeModeFilter
}
else {
    if (-not $MetaFile) {
        $MetaFile = Get-ChildItem (Join-Path $scriptDir 'exports') -Filter 'missions_export_meta_*.json' -ErrorAction SilentlyContinue |
            Sort-Object LastWriteTime -Descending | Select-Object -First 1 -ExpandProperty FullName
    }
    if (-not $MetaFile) { throw 'MetaFile required for Source=Export (no export found in ops/dash2a-readiness/exports)' }
    Write-Host "Loading from export $MetaFile"
    $loaded = Load-SessionsFromExport -Path $MetaFile
    $exportMeta = $loaded.meta
    $exportDir = $loaded.exportDir
    $sessions = $loaded.sessions
}

if ($sessions.Count -eq 0) { throw 'No candidate sessions found.' }
if ($MaxMissions -gt 0) {
    $sessions = @($sessions | Select-Object -First $MaxMissions)
}

$samplesBySession = if ($Source -eq 'LocalDb') {
    Load-SamplesFromLocalDb -Server $LocalServer -Db $LocalDb -SessionIds @($sessions | ForEach-Object { [int]$_.sourceId })
} else {
    Load-SamplesFromExport -ExportDir $exportDir -Meta $exportMeta
}

$plan = Build-RegeneratedPlan -Sessions $sessions -StartDate $SequenceStartDate -GapMinutes $SequenceGapMinutes -RegenDates $doRegenerateDates

$candidates = @()
foreach ($p in $plan) {
    $sid = [int]$p.sourceId
    $sc = if ($samplesBySession.ContainsKey($sid)) { $samplesBySession[$sid].Count } else { 0 }
    $pObj = [ordered]@{}
    foreach ($k in $p.Keys) { $pObj[$k] = $p[$k] }
    $pObj['sampleCount'] = $sc
    $pObj['action'] = if ($Mode -eq 'DryRun') { 'would_insert' } else { 'pending' }
    $candidates += $pObj
}

$totalCandidateSamples = 0
foreach ($c in $candidates) { $totalCandidateSamples += [int]$c.sampleCount }

$dryRun = ($Mode -eq 'DryRun')
$report = [ordered]@{
    runId = $runId
    mode = $Mode
    source = $Source
    runtimeModeFilter = $RuntimeModeFilter
    regenerateDates = $doRegenerateDates
    oneByOne = $doOneByOne
    sequenceStartDate = $SequenceStartDate.ToString('o')
    sequenceGapMinutes = $SequenceGapMinutes
    dryRun = $dryRun
    candidateCount = $candidates.Count
    candidateSamples = $totalCandidateSamples
    prodBefore = @{}
    prodAfter = @{}
    missions = @()
    errors = @()
    rollbackArtifacts = @()
}

# --- Production connection (DryRun reads prod state; Apply writes) ---
$prodCs = $null
$conn = $null
try {
    $prodCs = Get-ProdConnString -ConfigPath $ProdConfigPath
    $conn = Open-Conn $prodCs
}
catch {
    if ($dryRun) {
        Write-Warning "Prod config unavailable locally: $($_.Exception.Message). Plan-only dry-run, no prod gate checks."
    }
    else { throw }
}

if ($conn) {
    $openCount = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionSessions WHERE Completed = 0')
    if ($openCount -gt 0 -and -not $dryRun) {
        throw "Refusing Apply: $openCount open missions in production."
    }
    if ($openCount -gt 0 -and $dryRun) {
        Write-Warning "Prod has $openCount open missions. Apply would be blocked until finalized."
    }
    $report.prodBefore.sessions = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionSessions')
    $report.prodBefore.samples = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionMarginSamples')
    $report.prodBefore.openMissions = $openCount

    $existingKeys = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)
    $keyReader = $conn.CreateCommand()
    $keyReader.CommandText = 'SELECT MissionKey FROM dbo.MissionSessions WHERE MissionKey IS NOT NULL'
    $kr = $keyReader.ExecuteReader()
    while ($kr.Read()) { [void]$existingKeys.Add([string]$kr[0]) }
    $kr.Close()
}
else {
    $existingKeys = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)
}

Write-Host "Candidates: $($candidates.Count) missions, $($report.candidateSamples) samples, Mode=$Mode RegenerateDates=$doRegenerateDates"

foreach ($item in $candidates) {
    $missionLog = [ordered]@{
        sequence = $item.sequence
        sourceId = $item.sourceId
        sourceMissionKey = $item.sourceMissionKey
        newMissionKey = $item.newMissionKey
        newStartTime = $item.newStartTime
        newEndTime = $item.newEndTime
        sampleCount = $item.sampleCount
        status = 'pending'
    }

    if ($existingKeys.Contains([string]$item.newMissionKey)) {
        $missionLog.status = 'skipped'
        $missionLog.reason = 'MissionKey already exists in production'
        $report.missions += $missionLog
        Write-Warning "Skip seq $($item.sequence) source $($item.sourceId): key exists"
        continue
    }

    $sourceSamples = if ($samplesBySession.ContainsKey([int]$item.sourceId)) { $samplesBySession[[int]$item.sourceId] } else { $null }
    $oldStart = [datetime]::Parse($item.sourceStartTime)
    $newStart = [datetime]::Parse($item.newStartTime)
    $shiftedSamples = Shift-SampleTimestamps -Samples $sourceSamples -OldStart $oldStart -NewStart $newStart

    if ($dryRun -or -not $conn) {
        $missionLog.status = 'dry_run_ok'
        $missionLog.shiftedSampleCount = $shiftedSamples.Count
        $missionLog.rollbackSqlPlanned = "rollback/rollback_seq_$($item.sequence.ToString('D4'))_source_$($item.sourceId).sql"
        Write-RollbackSql -Path (Join-Path $rollbackDir "rollback_seq_$($item.sequence.ToString('D4'))_source_$($item.sourceId)_DRYRUN.sql") `
            -ProdSessionId 0 -MissionKey $item.newMissionKey -SampleCount $shiftedSamples.Count
        $report.missions += $missionLog
        Write-Host "[DRY-RUN] seq $($item.sequence) source $($item.sourceId) -> key $($item.newMissionKey) samples=$($shiftedSamples.Count)"
        continue
    }

    # --- Apply: one transaction per mission ---
    $beforeSessions = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionSessions')
    $beforeSamples = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionMarginSamples')

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
        $now = [datetime]::UtcNow
        [void]$insert.Parameters.AddWithValue('@MissionKey', [string]$item.newMissionKey)
        [void]$insert.Parameters.AddWithValue('@StartTime', $newStart)
        [void]$insert.Parameters.AddWithValue('@EndTime', $(if ($item.newEndTime) { [datetime]::Parse($item.newEndTime) } else { [DBNull]::Value }))
        [void]$insert.Parameters.AddWithValue('@TotalMargin', [decimal]$item.totalMargin)
        [void]$insert.Parameters.AddWithValue('@RealHandsCount', [int]$item.realHandsCount)
        [void]$insert.Parameters.AddWithValue('@LastTotalMarginForRealHands', $(if ($null -ne $item.lastTotalMarginForRealHands) { [decimal]$item.lastTotalMarginForRealHands } else { [DBNull]::Value }))
        [void]$insert.Parameters.AddWithValue('@GlobalTarget', [decimal]$item.globalTarget)
        [void]$insert.Parameters.AddWithValue('@ActiveTables', [int]$item.activeTables)
        [void]$insert.Parameters.AddWithValue('@KFactor', [decimal]$item.kFactor)
        [void]$insert.Parameters.AddWithValue('@RuntimeMode', 'Production')
        [void]$insert.Parameters.AddWithValue('@Completed', $true)
        [void]$insert.Parameters.AddWithValue('@ReportPublishedAt', $now)
        [void]$insert.Parameters.AddWithValue('@FinalizationReason', 'OneByOneHistoricalImport')
        [void]$insert.Parameters.AddWithValue('@CreatedAt', $now)

        $newId = [int]$insert.ExecuteScalar()
        [void]$existingKeys.Add([string]$item.newMissionKey)

        $batch = New-Object System.Data.DataTable
        [void]$batch.Columns.Add('SessionId', [int])
        [void]$batch.Columns.Add('Timestamp', [datetime])
        [void]$batch.Columns.Add('TotalMargin', [decimal])
        [void]$batch.Columns.Add('ActiveTables', [int])
        [void]$batch.Columns.Add('VmCurrent', [decimal])
        [void]$batch.Columns.Add('RuntimeMode', [string])

        $insertedSamples = 0
        foreach ($sample in $shiftedSamples) {
            $row = $batch.NewRow()
            $row['SessionId'] = $newId
            $row['Timestamp'] = [datetime]::Parse($sample.timestamp)
            $row['TotalMargin'] = [decimal]$sample.totalMargin
            $row['ActiveTables'] = [int]$sample.activeTables
            $row['VmCurrent'] = [decimal]$sample.vmCurrent
            $row['RuntimeMode'] = 'Production'
            [void]$batch.Rows.Add($row)
            $insertedSamples++

            if ($batch.Rows.Count -ge $SampleBatchSize) {
                $bulk = New-SampleBulkCopy -Conn $conn -Tx $tx -BatchSize $SampleBatchSize
                $bulk.WriteToServer($batch)
                $batch.Rows.Clear()
            }
        }
        if ($batch.Rows.Count -gt 0) {
            $bulk = New-SampleBulkCopy -Conn $conn -Tx $tx -BatchSize $SampleBatchSize
            $bulk.WriteToServer($batch)
        }

        $tx.Commit()

        $afterSessions = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionSessions')
        $afterSamples = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionMarginSamples')

        if ($afterSessions -ne ($beforeSessions + 1)) {
            throw "Post-insert session count mismatch: before=$beforeSessions after=$afterSessions"
        }
        if ($afterSamples -ne ($beforeSamples + $insertedSamples)) {
            throw "Post-insert sample count mismatch: before=$beforeSamples after=$afterSamples expected +$insertedSamples"
        }

        $rbPath = Join-Path $rollbackDir "rollback_seq_$($item.sequence.ToString('D4'))_prod_$newId.sql"
        Write-RollbackSql -Path $rbPath -ProdSessionId $newId -MissionKey $item.newMissionKey -SampleCount $insertedSamples

        $missionLog.status = 'inserted'
        $missionLog.newProdSessionId = $newId
        $missionLog.samplesInserted = $insertedSamples
        $missionLog.rollbackSql = $rbPath
        $report.rollbackArtifacts += $rbPath
        $report.missions += $missionLog

        Write-Host "[APPLY OK] seq $($item.sequence) source $($item.sourceId) -> prodId=$newId samples=$insertedSamples"

        if ($doOneByOne) {
            Start-Sleep -Milliseconds 500
        }
    }
    catch {
        $tx.Rollback()
        $missionLog.status = 'failed'
        $missionLog.error = $_.Exception.Message
        $report.errors += $missionLog
        $report.missions += $missionLog
        Write-Warning "[FAILED] seq $($item.sequence) source $($item.sourceId): $($_.Exception.Message)"
        if ($doOneByOne) { throw "OneByOne stop on failure at sequence $($item.sequence)" }
    }
}

if ($conn) {
    $report.prodAfter.sessions = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionSessions')
    $report.prodAfter.samples = [int](Get-Scalar $conn 'SELECT COUNT(*) FROM dbo.MissionMarginSamples')
    $conn.Close()
}

$report.completedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
$summaryFile = Join-Path $logDir 'run_summary.json'
$candidateFile = Join-Path $logDir 'candidates.json'
$report | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $summaryFile -Encoding UTF8
$candidates | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $candidateFile -Encoding UTF8

Write-Host ''
Write-Host '=== ONE-BY-ONE RUN COMPLETE ==='
Write-Host "Mode: $Mode"
Write-Host "Log dir: $logDir"
Write-Host "Candidates: $($candidates.Count)"
Write-Host "Missions processed: $($report.missions.Count)"
Write-Host "Errors: $($report.errors.Count)"
if ($conn -or $report.prodBefore.sessions) {
    Write-Host "Prod sessions: $($report.prodBefore.sessions) -> $($report.prodAfter.sessions)"
    Write-Host "Prod samples:  $($report.prodBefore.samples) -> $($report.prodAfter.samples)"
}
Write-Host "Summary: $summaryFile"

if ($report.errors.Count -gt 0) { exit 2 }
