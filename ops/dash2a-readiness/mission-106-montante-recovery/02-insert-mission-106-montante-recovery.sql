/*
  DASH2A — Mission #106 Montante recovery — INSERT (DESTRUCTIVE)
  Requires sqlcmd variable: Confirm=INSERT_MISSION_106_MONTANTE

  Example:
    sqlcmd -v Confirm="INSERT_MISSION_106_MONTANTE" -i 02-insert-mission-106-montante-recovery.sql

  Does NOT modify mission #105.
*/
SET NOCOUNT ON;
SET DATEFORMAT ymd;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @Confirm nvarchar(64) = '$(Confirm)';

IF @Confirm <> N'INSERT_MISSION_106_MONTANTE'
BEGIN
    RAISERROR('Refusing: set -v Confirm="INSERT_MISSION_106_MONTANTE" after explicit human approval.', 16, 1);
    RETURN;
END

DECLARE @StartUtc       datetime2 = '2026-06-04T14:25:10';
DECLARE @EndUtc         datetime2 = '2026-06-04T18:01:20';
DECLARE @TotalMargin    decimal(18,2) = 340.60;
DECLARE @MissionKey     nvarchar(128) = N'montante-recovery-post105-20260604';
DECLARE @FinalReason    nvarchar(128) = N'MontanteRecovery';
DECLARE @RealHands      int = 1050;
DECLARE @ActiveTables   int = 4;
DECLARE @RuntimeMode    nvarchar(32) = N'Production';
DECLARE @KFactor        decimal(18,2) = 1.0;

DECLARE @GlobalTarget decimal(18,2) = (
    SELECT TOP 1 m.GlobalTarget FROM dbo.MissionSessions m WHERE m.ID = 105
);
IF @GlobalTarget IS NULL SET @GlobalTarget = 220.00;

PRINT '=== INSERT Mission #106 MontanteRecovery — BEGIN ===';

-- Guards
IF NOT EXISTS (
    SELECT 1 FROM dbo.MissionSessions m
    WHERE m.ID = 105 AND m.TotalMargin = 254.40 AND m.Completed = 1
      AND m.EndTime = '2026-06-04T14:25:10'
)
BEGIN
    RAISERROR('Refusing: mission #105 invariant failed (254.40 / EndTime / Completed).', 16, 1);
    RETURN;
END

IF EXISTS (SELECT 1 FROM dbo.MissionSessions m WHERE m.ID = 106)
BEGIN
    RAISERROR('Refusing: MissionSessions ID 106 already exists.', 16, 1);
    RETURN;
END

IF EXISTS (SELECT 1 FROM dbo.MissionSessions m WHERE m.MissionKey = @MissionKey)
BEGIN
    RAISERROR('Refusing: MissionKey already exists.', 16, 1);
    RETURN;
END

IF EXISTS (
    SELECT 1 FROM dbo.MissionSessions m
    WHERE m.FinalizationReason = @FinalReason AND m.StartTime = @StartUtc
)
BEGIN
    RAISERROR('Refusing: MontanteRecovery session for same StartTime already exists.', 16, 1);
    RETURN;
END

BEGIN TRANSACTION;

INSERT INTO dbo.MissionSessions (
    MissionKey,
    StartTime,
    EndTime,
    TotalMargin,
    RealHandsCount,
    LastTotalMarginForRealHands,
    GlobalTarget,
    ActiveTables,
    KFactor,
    RuntimeMode,
    Completed,
    ReportPublishedAt,
    FinalizationReason,
    CreatedAt
)
VALUES (
    @MissionKey,
    @StartUtc,
    @EndUtc,
    @TotalMargin,
    @RealHands,
    @TotalMargin,
    @GlobalTarget,
    @ActiveTables,
    @KFactor,
    @RuntimeMode,
    1,
    @EndUtc,
    @FinalReason,
    SYSUTCDATETIME()
);

DECLARE @NewId int = SCOPE_IDENTITY();

INSERT INTO dbo.MissionMarginSamples (
    SessionId,
    Timestamp,
    TotalMargin,
    ActiveTables,
    VmCurrent,
    RuntimeMode
)
VALUES
    (@NewId, @StartUtc, 0.00, @ActiveTables, 0.00, @RuntimeMode),
    (@NewId, @EndUtc, @TotalMargin, @ActiveTables, @TotalMargin, @RuntimeMode);

COMMIT TRANSACTION;

PRINT 'INSERT_OK SessionId=' + CAST(@NewId AS varchar(20));
PRINT 'MissionKey=' + @MissionKey;
PRINT 'TotalMargin=' + CAST(@TotalMargin AS varchar(20));
PRINT 'StartUtc=' + CONVERT(varchar(30), @StartUtc, 126);
PRINT 'EndUtc=' + CONVERT(varchar(30), @EndUtc, 126);
PRINT 'Run 03-verify-mission-106-montante-recovery.sql';
