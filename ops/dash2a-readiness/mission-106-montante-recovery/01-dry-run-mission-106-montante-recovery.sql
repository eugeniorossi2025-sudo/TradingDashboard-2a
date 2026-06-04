/*
  DASH2A — Mission #106 Montante recovery — DRY-RUN (READ ONLY)
  Post-#105 orphan window 2026-06-04. Nessun INSERT/UPDATE/DELETE.

  Validated totals:
    TotalMargin = +340.60 EUR (gross winning closed progressions)
    Net audit     = +278.55 EUR (84 progressions)
  #105 must remain: TotalMargin 254.40, EndTime 2026-06-04T14:25:10 (UTC)
*/
SET NOCOUNT ON;
SET DATEFORMAT ymd;

DECLARE @StartUtc       datetime2 = '2026-06-04T14:25:10';
DECLARE @EndUtc         datetime2 = '2026-06-04T18:01:20';
DECLARE @TotalMargin    decimal(18,2) = 340.60;
DECLARE @MissionKey     nvarchar(128) = N'montante-recovery-post105-20260604';
DECLARE @FinalReason    nvarchar(128) = N'MontanteRecovery';
DECLARE @AuditNet       decimal(18,2) = 278.55;
DECLARE @Progressions   int = 84;

PRINT '=== MISSION 106 MONTANTE RECOVERY — DRY RUN (READ ONLY) ===';
PRINT 'Rome Start (UTC+2): 2026-06-04 16:25:10 | UTC Start: ' + CONVERT(varchar(30), @StartUtc, 126);
PRINT 'Rome End   (UTC+2): 2026-06-04 20:01:20 | UTC End:   ' + CONVERT(varchar(30), @EndUtc, 126);
PRINT 'Proposed TotalMargin EUR: ' + CAST(@TotalMargin AS varchar(20));
PRINT '';

-- 1) #105 invariant
PRINT '=== 1) Mission #105 (must be unchanged — reference only) ===';
SELECT
    m.ID,
    m.MissionKey,
    m.StartTime,
    m.EndTime,
    m.TotalMargin,
    m.Completed,
    m.FinalizationReason,
    m.RuntimeMode,
    m.ActiveTables,
    m.RealHandsCount,
    m.GlobalTarget,
    m.KFactor,
    CASE
        WHEN m.ID = 105
         AND m.TotalMargin = 254.40
         AND m.Completed = 1
         AND m.EndTime = '2026-06-04T14:25:10'
         AND m.FinalizationReason LIKE N'%STOP_WIN%'
        THEN 'OK_105_UNTOUCHED'
        ELSE 'FAIL_105_EXPECTED'
    END AS Guard105
FROM dbo.MissionSessions m
WHERE m.ID = 105;

IF @@ROWCOUNT = 0
    PRINT 'FAIL: Mission #105 row missing.';

-- 2) Duplicate #106 / MissionKey
PRINT '';
PRINT '=== 2) Existing #106 or duplicate MissionKey ===';
SELECT
    m.ID,
    m.MissionKey,
    m.StartTime,
    m.EndTime,
    m.TotalMargin,
    m.FinalizationReason,
    CASE
        WHEN m.ID = 106 THEN 'ROW_ID_106_EXISTS'
        WHEN m.MissionKey = @MissionKey THEN 'DUPLICATE_MISSION_KEY'
        ELSE 'OTHER'
    END AS DuplicateFlag
FROM dbo.MissionSessions m
WHERE m.ID = 106
   OR m.MissionKey = @MissionKey
   OR (m.FinalizationReason = @FinalReason AND m.StartTime = @StartUtc);

SELECT
    COUNT(*) AS BlockingDuplicates,
    CASE WHEN COUNT(*) = 0 THEN 'OK_NO_BLOCKING_DUPLICATE' ELSE 'FAIL_DUPLICATE_EXISTS' END AS DuplicateCheck
FROM dbo.MissionSessions m
WHERE m.ID = 106
   OR m.MissionKey = @MissionKey
   OR (m.FinalizationReason = @FinalReason AND m.StartTime = @StartUtc);

-- 3) Next identity (informational)
PRINT '';
PRINT '=== 3) Next MissionSessions ID (IDENTITY) ===';
SELECT
    MAX(m.ID) AS MaxExistingId,
    MAX(m.ID) + 1 AS ExpectedNextId
FROM dbo.MissionSessions m;

-- 4) Chain #105 -> proposed #106
PRINT '';
PRINT '=== 4) Temporal chain #105 -> proposed #106 ===';
SELECT
    prev.ID AS PrevId,
    prev.EndTime AS PrevEndUtc,
    @StartUtc AS ProposedStartUtc,
    @EndUtc AS ProposedEndUtc,
    DATEDIFF(SECOND, prev.EndTime, @StartUtc) AS GapSeconds_StartMinusPrevEnd,
    DATEDIFF(SECOND, @StartUtc, @EndUtc) AS DurationSeconds,
    CASE
        WHEN prev.EndTime = @StartUtc THEN 'ABUT_SEAMLESS'
        WHEN prev.EndTime < @StartUtc THEN 'GAP'
        ELSE 'OVERLAP'
    END AS ChainStatus
FROM dbo.MissionSessions prev
WHERE prev.ID = 105;

-- 5) Proposed INSERT row (preview)
PRINT '';
PRINT '=== 5) Proposed INSERT into MissionSessions (preview only) ===';
DECLARE @GlobalTarget decimal(18,2) = (
    SELECT TOP 1 m.GlobalTarget FROM dbo.MissionSessions m WHERE m.ID = 105
);
IF @GlobalTarget IS NULL SET @GlobalTarget = 220.00;

SELECT
    CAST(NULL AS int) AS ID_IDENTITY_ASSIGNED,
    @MissionKey AS MissionKey,
    @StartUtc AS StartTime,
    @EndUtc AS EndTime,
    @TotalMargin AS TotalMargin,
    1050 AS RealHandsCount,
    @TotalMargin AS LastTotalMarginForRealHands,
    @GlobalTarget AS GlobalTarget,
    4 AS ActiveTables,
    CAST(1.0 AS decimal(18,2)) AS KFactor,
    N'Production' AS RuntimeMode,
    CAST(1 AS bit) AS Completed,
    @EndUtc AS ReportPublishedAt,
    @FinalReason AS FinalizationReason,
    SYSUTCDATETIME() AS CreatedAt_Recommended;

PRINT '';
PRINT '=== 6) Proposed MissionMarginSamples (2 points, preview) ===';
SELECT *
FROM (
    SELECT
        1 AS SortOrder,
        CAST(NULL AS int) AS SessionId_Placeholder_106,
        @StartUtc AS Timestamp,
        CAST(0.00 AS decimal(18,2)) AS TotalMargin,
        4 AS ActiveTables,
        CAST(0.00 AS decimal(18,2)) AS VmCurrent,
        N'Production' AS RuntimeMode
    UNION ALL
    SELECT
        2,
        NULL,
        @EndUtc,
        @TotalMargin,
        4,
        @TotalMargin,
        N'Production'
) x
ORDER BY x.SortOrder;

-- 7) Validation matrix
PRINT '';
PRINT '=== 7) DRY-RUN validation matrix ===';
SELECT
    v.CheckCode,
    v.Expected,
    v.Actual,
    v.Pass
FROM (
    SELECT N'StartTime_Rome' AS CheckCode, N'2026-06-04 16:25:10' AS Expected,
           CONVERT(varchar(19), DATEADD(HOUR, 2, @StartUtc), 126) AS Actual,
           CASE WHEN @StartUtc = '2026-06-04T14:25:10' THEN N'PASS' ELSE N'FAIL' END AS Pass
    UNION ALL
    SELECT N'EndTime_Rome', N'2026-06-04 20:01:20',
           CONVERT(varchar(19), DATEADD(HOUR, 2, @EndUtc), 126),
           CASE WHEN @EndUtc = '2026-06-04T18:01:20' THEN N'PASS' ELSE N'FAIL' END
    UNION ALL
    SELECT N'TotalMargin_EUR', N'340.60', CAST(@TotalMargin AS varchar(20)),
           CASE WHEN @TotalMargin = 340.60 THEN N'PASS' ELSE N'FAIL' END
    UNION ALL
    SELECT N'Mission105_TotalMargin', N'254.40',
           CAST((SELECT TOP 1 m.TotalMargin FROM dbo.MissionSessions m WHERE m.ID = 105) AS varchar(20)),
           CASE WHEN EXISTS (SELECT 1 FROM dbo.MissionSessions m WHERE m.ID = 105 AND m.TotalMargin = 254.40) THEN N'PASS' ELSE N'FAIL' END
    UNION ALL
    SELECT N'No_Id_106', N'0 rows',
           CAST((SELECT COUNT(*) FROM dbo.MissionSessions m WHERE m.ID = 106) AS varchar(20)),
           CASE WHEN NOT EXISTS (SELECT 1 FROM dbo.MissionSessions m WHERE m.ID = 106) THEN N'PASS' ELSE N'FAIL' END
    UNION ALL
    SELECT N'No_Duplicate_MissionKey', N'0 rows',
           CAST((SELECT COUNT(*) FROM dbo.MissionSessions m WHERE m.MissionKey = @MissionKey) AS varchar(20)),
           CASE WHEN NOT EXISTS (SELECT 1 FROM dbo.MissionSessions m WHERE m.MissionKey = @MissionKey) THEN N'PASS' ELSE N'FAIL' END
) v
ORDER BY v.CheckCode;

PRINT '';
PRINT 'Audit trail (not stored in DB by this script): Net=' + CAST(@AuditNet AS varchar(20))
    + ' EUR | Progressions=' + CAST(@Progressions AS varchar(10));
PRINT 'DRY-RUN complete. No data modified.';
PRINT 'Next: after explicit approval, run 02-insert with -v Confirm="INSERT_MISSION_106_MONTANTE"';
