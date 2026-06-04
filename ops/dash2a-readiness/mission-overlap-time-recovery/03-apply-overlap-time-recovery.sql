/*
  DASH2A — Applica bonifica finestre temporali #101-#104.
  Modifica SOLO: EndTime (101-103), FinalizationReason (101-103).
  #104: EndTime e FinalizationReason invariati (ResetDashboard).
  Prerequisiti: 01 dry-run OK, 02 backup eseguito.
*/
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @Ids TABLE (ID int PRIMARY KEY);
INSERT INTO @Ids (ID) VALUES (101), (102), (103), (104);

IF EXISTS (
    SELECT 1
    FROM dbo.MissionSessions m
    INNER JOIN @Ids i ON i.ID = m.ID
    WHERE m.Completed = 0
)
BEGIN
    RAISERROR('Refusing apply: one or more sessions 101-104 are not Completed.', 16, 1);
    ROLLBACK TRANSACTION;
    RETURN;
END

IF (SELECT COUNT(*) FROM dbo.MissionSessions m INNER JOIN @Ids i ON i.ID = m.ID) <> 4
BEGIN
    RAISERROR('Refusing apply: expected exactly 4 MissionSessions rows (101-104).', 16, 1);
    ROLLBACK TRANSACTION;
    RETURN;
END

;WITH chain AS (
    SELECT
        m.ID,
        m.EndTime AS CurrentEndTime,
        LEAD(m.StartTime) OVER (ORDER BY m.ID) AS NextStartTime
    FROM dbo.MissionSessions m
    INNER JOIN @Ids i ON i.ID = m.ID
)
UPDATE m
SET
    m.EndTime = c.NextStartTime,
    m.FinalizationReason = N'OverlapTimeRecovery'
FROM dbo.MissionSessions m
INNER JOIN chain c ON c.ID = m.ID
WHERE m.ID IN (101, 102, 103)
  AND c.NextStartTime IS NOT NULL
  AND (
      m.EndTime <> c.NextStartTime
      OR m.EndTime IS NULL
      OR ISNULL(m.FinalizationReason, N'') <> N'OverlapTimeRecovery'
  );

DECLARE @updated int = @@ROWCOUNT;
PRINT 'UPDATED_ROWS_101_103=' + CAST(@updated AS varchar(20));

COMMIT TRANSACTION;

PRINT 'Apply committed. Run 04-verify-overlap-time-recovery.sql';
