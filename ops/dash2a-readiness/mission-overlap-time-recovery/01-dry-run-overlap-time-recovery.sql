/*
  DASH2A — Bonifica finestre temporali missioni #101-#104 (READ ONLY)
  Obiettivo: EndTime(N) = StartTime(N+1); StartTime invariati; #104 EndTime invariato.
  Non tocca: MissionMarginSamples, TotalMargin, RealHandsCount, altri campi economici.
*/
SET NOCOUNT ON;

DECLARE @Ids TABLE (ID int PRIMARY KEY);
INSERT INTO @Ids (ID) VALUES (101), (102), (103), (104);

;WITH chain AS (
    SELECT
        m.ID,
        m.MissionKey,
        m.StartTime,
        m.EndTime,
        m.Completed,
        m.FinalizationReason,
        m.TotalMargin,
        m.RealHandsCount,
        LEAD(m.StartTime) OVER (ORDER BY m.ID) AS NextStartTime
    FROM dbo.MissionSessions m
    INNER JOIN @Ids i ON i.ID = m.ID
),
proposed AS (
    SELECT
        c.*,
        c.StartTime AS ProposedStartTime,
        CASE
            WHEN c.ID = 104 THEN c.EndTime
            ELSE c.NextStartTime
        END AS ProposedEndTime,
        CASE
            WHEN c.ID IN (101, 102, 103) THEN N'OverlapTimeRecovery'
            ELSE c.FinalizationReason
        END AS ProposedFinalizationReason
    FROM chain c
)
SELECT
    p.ID,
    p.MissionKey,
    p.Completed,
    p.TotalMargin,
    p.RealHandsCount,
    p.FinalizationReason AS CurrentFinalizationReason,
    p.ProposedFinalizationReason,
    p.StartTime AS CurrentStartTime,
    p.EndTime AS CurrentEndTime,
    p.ProposedStartTime,
    p.ProposedEndTime,
    DATEDIFF(SECOND, p.StartTime, p.EndTime) AS CurrentDurationSec,
    DATEDIFF(SECOND, p.ProposedStartTime, p.ProposedEndTime) AS ProposedDurationSec,
    CASE WHEN p.StartTime <> p.ProposedStartTime THEN 1 ELSE 0 END AS StartWillChange,
    CASE WHEN p.EndTime <> p.ProposedEndTime OR (p.EndTime IS NULL AND p.ProposedEndTime IS NOT NULL) OR (p.EndTime IS NOT NULL AND p.ProposedEndTime IS NULL) THEN 1 ELSE 0 END AS EndWillChange,
    CASE WHEN p.FinalizationReason <> p.ProposedFinalizationReason OR (p.FinalizationReason IS NULL AND p.ProposedFinalizationReason IS NOT NULL) THEN 1 ELSE 0 END AS ReasonWillChange
FROM proposed p
ORDER BY p.ID;

PRINT '';
PRINT '=== Overlap pairs BEFORE (End > Next.Start) ===';
;WITH ordered AS (
    SELECT m.ID, m.StartTime, m.EndTime
    FROM dbo.MissionSessions m
    INNER JOIN @Ids i ON i.ID = m.ID
)
SELECT
    a.ID AS SessionId,
    b.ID AS NextSessionId,
    a.EndTime AS CurrentEnd,
    b.StartTime AS NextStart,
    DATEDIFF(SECOND, b.StartTime, a.EndTime) AS OverlapSeconds
FROM ordered a
INNER JOIN ordered b ON b.ID = a.ID + 1
WHERE a.EndTime > b.StartTime
ORDER BY a.ID;

PRINT '';
PRINT '=== Chain gaps/overlaps AFTER (simulated) ===';
;WITH chain AS (
    SELECT
        m.ID,
        m.StartTime,
        CASE WHEN m.ID = 104 THEN m.EndTime ELSE LEAD(m.StartTime) OVER (ORDER BY m.ID) END AS ProposedEndTime
    FROM dbo.MissionSessions m
    INNER JOIN @Ids i ON i.ID = m.ID
)
SELECT
    a.ID AS SessionId,
    b.ID AS NextSessionId,
    a.ProposedEndTime,
    b.StartTime AS NextStart,
    DATEDIFF(SECOND, b.StartTime, a.ProposedEndTime) AS DeltaSeconds_NextStartMinusEnd,
    CASE
        WHEN a.ProposedEndTime > b.StartTime THEN 'OVERLAP'
        WHEN a.ProposedEndTime < b.StartTime THEN 'GAP'
        ELSE 'ABUT'
    END AS ChainStatus
FROM chain a
INNER JOIN chain b ON b.ID = a.ID + 1
ORDER BY a.ID;

PRINT '';
PRINT '=== Sample counts (unchanged by this script) ===';
SELECT
    m.ID,
    COUNT_BIG(s.ID) AS SampleCount,
    MIN(s.Timestamp) AS FirstSample,
    MAX(s.Timestamp) AS LastSample
FROM dbo.MissionSessions m
INNER JOIN @Ids i ON i.ID = m.ID
LEFT JOIN dbo.MissionMarginSamples s ON s.SessionId = m.ID
GROUP BY m.ID
ORDER BY m.ID;
