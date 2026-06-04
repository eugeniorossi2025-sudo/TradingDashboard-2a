-- DASH2A PROD readonly — orphan window 2026-06-04 14:25:10–18:32:29 Europe/Rome
-- Eseguire su Eugenio-Demo10 @ 51.83.159.175,1434 (runner DASH2A-BACKEND). SELECT only.

SET NOCOUNT ON;

DECLARE @OrphanStart datetime2 = '2026-06-04T14:25:10';
DECLARE @OrphanEnd   datetime2 = '2026-06-04T18:32:29';

-- 1) Configurations (prove suppress + reset boundary)
SELECT [K], Value, Description
FROM dbo.Configurations
WHERE [K] IN (
    N'MISSION_SUPPRESS_START_UNTIL_RESET',
    N'MISSION_LAST_RESET_AT_UTC',
    N'MISSION_ACCOUNTING_RECOVERY_AT_UTC'
);

-- 2) Mission #105 close + no #106
SELECT Id, StartTime, EndTime, TotalMargin, Completed, FinalizationReason, MissionKey, RuntimeMode
FROM dbo.MissionSessions
WHERE Id >= 101 AND Id <= 110
ORDER BY Id;

-- 3) Sessions started inside orphan window (must be zero after #105)
SELECT Id, StartTime, EndTime, TotalMargin, Completed, FinalizationReason
FROM dbo.MissionSessions
WHERE StartTime > @OrphanStart AND StartTime <= @OrphanEnd
ORDER BY StartTime;

-- 4) dbo.Margini — TUTTE le righe nella finestra orfana (colonna SQL = Margine)
SELECT COUNT(*) AS MarginiCountInOrphan,
       MIN(Data) AS FirstData,
       MAX(Data) AS LastData,
       MIN(Margine) AS MinMargin,
       MAX(Margine) AS MaxMargin
FROM dbo.Margini
WHERE Data >= @OrphanStart AND Data <= @OrphanEnd;

SELECT ID, Data, Margine
FROM dbo.Margini
WHERE Data >= @OrphanStart AND Data <= @OrphanEnd
ORDER BY Data;

-- 4b) Margini contesto 14:20–18:40 (per verifica bordo finestra)
SELECT ID, Data, Margine
FROM dbo.Margini
WHERE Data >= '2026-06-04T14:20:00' AND Data <= '2026-06-04T18:40:00'
ORDER BY Data;

-- 5) Last Margini before orphan (expected ~254.4 at #105 close)
SELECT TOP 5 ID, Data, Margine
FROM dbo.Margini
WHERE Data < @OrphanStart
ORDER BY Data DESC;

-- 5b) MissionMarginSamples — TUTTE le righe nel intervallo Timestamp (qualsiasi SessionId)
SELECT COUNT(*) AS SamplesCountInOrphan
FROM dbo.MissionMarginSamples
WHERE Timestamp >= @OrphanStart AND Timestamp <= @OrphanEnd;

SELECT ID, SessionId, Timestamp, TotalMargin, ActiveTables, RuntimeMode
FROM dbo.MissionMarginSamples
WHERE Timestamp >= @OrphanStart AND Timestamp <= @OrphanEnd
ORDER BY Timestamp, SessionId;

-- 6) #105 samples vs Margini alignment (last sample = TotalMargin)
SELECT TOP 5 Timestamp, TotalMargin
FROM dbo.MissionMarginSamples
WHERE SessionId = 105
ORDER BY Timestamp DESC;

-- 7) Recovery TotalMargin preview (same rule as FinalizeSessionAsync: last Margini <= @OrphanEnd)
SELECT TOP 1
    @OrphanEnd AS OrphanEndRome,
    Data AS LastMarginiTimestamp,
    Margine AS RecoveryTotalMargin_Algorithm
FROM dbo.Margini
WHERE Data <= @OrphanEnd
ORDER BY Data DESC;

-- 8) Recovery NetPnl preview (same rule as MissionReportBuilder: last - first Margini in (start, end])
SELECT
    (SELECT TOP 1 Margine FROM dbo.Margini WHERE Data >= @OrphanStart AND Data <= @OrphanEnd ORDER BY Data ASC) AS FirstMarginInWindow,
    (SELECT TOP 1 Margine FROM dbo.Margini WHERE Data >= @OrphanStart AND Data <= @OrphanEnd ORDER BY Data DESC) AS LastMarginInWindow,
    (SELECT TOP 1 Margine FROM dbo.Margini WHERE Data >= @OrphanStart AND Data <= @OrphanEnd ORDER BY Data DESC)
    - (SELECT TOP 1 Margine FROM dbo.Margini WHERE Data >= @OrphanStart AND Data <= @OrphanEnd ORDER BY Data ASC) AS RecoveryNetPnl_Algorithm;
