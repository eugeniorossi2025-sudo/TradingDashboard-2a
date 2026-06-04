/*
  DASH2A — Verifica post-bonifica #101-#104
*/
SET NOCOUNT ON;

DECLARE @Ids TABLE (ID int PRIMARY KEY);
INSERT INTO @Ids (ID) VALUES (101), (102), (103), (104);

PRINT '=== Sessions 101-104 (final windows) ===';
SELECT
    ID,
    MissionKey,
    StartTime,
    EndTime,
    CAST(Completed AS int) AS CompletedInt,
    FinalizationReason,
    TotalMargin,
    RealHandsCount,
    DATEDIFF(SECOND, StartTime, EndTime) AS DurationSec
FROM dbo.MissionSessions
WHERE ID IN (101, 102, 103, 104)
ORDER BY ID;

PRINT '';
PRINT '=== Overlap check (must return 0 rows) ===';
;WITH ordered AS (
    SELECT m.ID, m.StartTime, m.EndTime
    FROM dbo.MissionSessions m
    INNER JOIN @Ids i ON i.ID = m.ID
)
SELECT
    a.ID AS SessionId,
    b.ID AS NextSessionId,
    a.EndTime,
    b.StartTime AS NextStart,
    DATEDIFF(SECOND, b.StartTime, a.EndTime) AS OverlapSeconds
FROM ordered a
INNER JOIN ordered b ON b.ID = a.ID + 1
WHERE a.EndTime > b.StartTime;

PRINT '';
PRINT '=== Consecutive chain (End = Next.Start, must be ABUT for 101-103) ===';
;WITH ordered AS (
    SELECT m.ID, m.StartTime, m.EndTime
    FROM dbo.MissionSessions m
    INNER JOIN @Ids i ON i.ID = m.ID
)
SELECT
    a.ID AS SessionId,
    b.ID AS NextSessionId,
    a.EndTime,
    b.StartTime AS NextStart,
    DATEDIFF(SECOND, a.EndTime, b.StartTime) AS EndToNextStartDeltaSec,
    CASE
        WHEN a.EndTime = b.StartTime THEN 'ABUT'
        WHEN a.EndTime < b.StartTime THEN 'GAP'
        ELSE 'OVERLAP'
    END AS ChainStatus
FROM ordered a
INNER JOIN ordered b ON b.ID = a.ID + 1
ORDER BY a.ID;

PRINT '';
PRINT '=== Completed flag (all must be 1) ===';
SELECT
    SUM(CASE WHEN Completed = 1 THEN 1 ELSE 0 END) AS CompletedTrue,
    SUM(CASE WHEN Completed = 0 THEN 1 ELSE 0 END) AS CompletedFalse
FROM dbo.MissionSessions
WHERE ID IN (101, 102, 103, 104);

PRINT '';
PRINT '=== Sample integrity (counts unchanged vs backup if needed) ===';
SELECT
    m.ID,
    COUNT_BIG(s.ID) AS SampleCount
FROM dbo.MissionSessions m
LEFT JOIN dbo.MissionMarginSamples s ON s.SessionId = m.ID
WHERE m.ID IN (101, 102, 103, 104)
GROUP BY m.ID
ORDER BY m.ID;
