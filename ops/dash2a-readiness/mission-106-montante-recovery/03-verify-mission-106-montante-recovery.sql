/*
  DASH2A — Verify mission #106 after INSERT (READ ONLY)
*/
SET NOCOUNT ON;
SET DATEFORMAT ymd;

PRINT '=== Verify #105 unchanged ===';
SELECT m.ID, m.TotalMargin, m.EndTime, m.FinalizationReason,
       CASE WHEN m.TotalMargin = 254.40 AND m.Completed = 1 THEN 'OK' ELSE 'FAIL' END AS Check105
FROM dbo.MissionSessions m WHERE m.ID = 105;

PRINT '=== Verify #106 ===';
SELECT m.*
FROM dbo.MissionSessions m
WHERE m.ID = 106 OR m.MissionKey = N'montante-recovery-post105-20260604';

PRINT '=== Samples #106 ===';
SELECT s.*
FROM dbo.MissionMarginSamples s
INNER JOIN dbo.MissionSessions m ON m.ID = s.SessionId
WHERE m.MissionKey = N'montante-recovery-post105-20260604'
ORDER BY s.Timestamp;

PRINT '=== Chain 105 -> 106 ===';
SELECT
    a.ID, a.EndTime AS End105,
    b.ID, b.StartTime AS Start106, b.EndTime AS End106, b.TotalMargin,
    DATEDIFF(SECOND, a.EndTime, b.StartTime) AS GapSec
FROM dbo.MissionSessions a
JOIN dbo.MissionSessions b ON b.ID = 106
WHERE a.ID = 105;
