using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using Entities;
using WebApi.Constants;
using WebApi.Data;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("api/mission")]
[Produces("application/json")]
[Authorize]
public class MissionController : ControllerBase
{
    private const string Production = "Production";
    private const string Demo = "Demo";
    private readonly AppDbContext _context;
    private readonly IMissionLifecycleService _missionLifecycleService;
    private readonly IMissionReportBuilder _missionReportBuilder;

    public MissionController(
        AppDbContext context,
        IMissionLifecycleService missionLifecycleService,
        IMissionReportBuilder missionReportBuilder)
    {
        _context = context;
        _missionLifecycleService = missionLifecycleService;
        _missionReportBuilder = missionReportBuilder;
    }

    [HttpGet("current")]
    [ProducesResponseType(typeof(ApiResponse<MissionLifecycleState>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
    {
        var current = await _missionLifecycleService.GetCurrentAsync(cancellationToken);
        return Ok(ApiResponse<MissionLifecycleState>.SuccessResponse(current));
    }

    [HttpGet("accounting-health")]
    [ProducesResponseType(typeof(ApiResponse<MissionAccountingHealth>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccountingHealth(CancellationToken cancellationToken)
    {
        var health = await _missionLifecycleService.GetAccountingHealthAsync(cancellationToken);
        return Ok(ApiResponse<MissionAccountingHealth>.SuccessResponse(health));
    }

    [HttpPost("start-current")]
    [Authorize(Policy = AuthConstants.Policies.RequireAdmin)]
    [ProducesResponseType(typeof(ApiResponse<MissionLifecycleResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> StartCurrent(CancellationToken cancellationToken)
    {
        var result = await _missionLifecycleService.StartCurrentAsync(cancellationToken);
        return result.Success
            ? Ok(ApiResponse<MissionLifecycleResult>.SuccessResponse(result, result.Message))
            : BadRequest(ApiResponse<MissionLifecycleResult>.ErrorResponse(result.Message));
    }

    [HttpPost("finalize-current")]
    [Authorize(Policy = AuthConstants.Policies.RequireAdmin)]
    [ProducesResponseType(typeof(ApiResponse<MissionLifecycleResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> FinalizeCurrent([FromBody] FinalizeMissionRequest? request, CancellationToken cancellationToken)
    {
        var result = await _missionLifecycleService.FinalizeCurrentAsync(request?.Reason ?? "ManualFinalize", cancellationToken);
        return Ok(ApiResponse<MissionLifecycleResult>.SuccessResponse(result, result.Message));
    }

    [HttpGet("report/range")]
    [ProducesResponseType(typeof(ApiResponse<MissionRangeReportResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRangeReport(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? runtimeMode,
        [FromQuery] string? format = "json",
        [FromQuery] bool summary = true)
    {
        var mode = _missionReportBuilder.NormalizeMode(runtimeMode);
        var fromDate = (from ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
        var toDateExclusive = (to ?? DateTime.Today).Date.AddDays(1);

        await EnsureMissionReportSchemaAsync();
        var report = await _missionReportBuilder.BuildRangeReportAsync(fromDate, toDateExclusive, mode);

        if (string.Equals(format, "html", StringComparison.OrdinalIgnoreCase))
        {
            var html = MissionReportHtmlBuilder.Build(report);
            return File(Encoding.UTF8.GetBytes(html), "text/html; charset=utf-8", enableRangeProcessing: false);
        }

        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = _missionReportBuilder.BuildCsv(report);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"mission_report_{mode}_{fromDate:yyyyMMdd}_{toDateExclusive.AddDays(-1):yyyyMMdd}.csv");
        }

        if (summary)
        {
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                report.RuntimeMode,
                report.IsDemoMode,
                report.From,
                report.To,
                report.GeneratedAt,
                report.Totals,
                report.QualityMetrics,
                DailyRowCount = report.DailyRows.Count,
                SampleCount = report.Samples.Count
            }));
        }

        return Ok(ApiResponse<MissionRangeReportResponse>.SuccessResponse(report));
    }

    [HttpGet("reports/index")]
    [ProducesResponseType(typeof(ApiResponse<MissionReportsIndexResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReportsIndex(
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] string? runtimeMode,
        [FromQuery] int? sessionId,
        [FromQuery] int limit = 100,
        [FromQuery] int skip = 0,
        [FromQuery] bool completedOnly = true)
    {
        var mode = _missionReportBuilder.NormalizeMode(runtimeMode);
        var includeAllModes = string.Equals(runtimeMode, "All", StringComparison.OrdinalIgnoreCase);
        var fromDate = (fromUtc ?? DateTime.UtcNow.AddYears(-10)).Date;
        var toExclusive = (toUtc ?? DateTime.UtcNow).Date.AddDays(1);
        await EnsureMissionReportSchemaAsync();

        limit = Math.Clamp(limit, 1, 500);
        skip = Math.Max(0, skip);

        var (periodStartUtc, periodEndUtc) = _missionReportBuilder.GetPeriodBoundsUtc(fromDate, toExclusive);

        var query = _missionReportBuilder.ApplyAccountingPeriodSessionFilterWithSamples(
            _context.MissionSessions.AsNoTracking(),
            periodStartUtc,
            periodEndUtc);

        if (!includeAllModes)
            query = query.Where(session => session.RuntimeMode == mode);

        if (completedOnly)
            query = query.Where(session => session.Completed);

        if (sessionId.HasValue)
            query = query.Where(session => session.Id == sessionId.Value);

        var total = await query.CountAsync();
        var sessions = await query
            .OrderByDescending(session => session.EndTime ?? session.StartTime)
            .ThenByDescending(session => session.Id)
            .Skip(skip)
            .Take(limit)
            .Select(session => new
            {
                session.Id,
                session.StartTime,
                session.EndTime,
                session.Completed,
                session.TotalMargin,
                session.GlobalTarget,
                session.KFactor,
                session.ActiveTables,
                session.RuntimeMode,
                session.RealHandsCount
            })
            .ToListAsync();

        var ids = sessions.Select(session => session.Id).ToArray();
        var sampleCounts = ids.Length == 0
            ? new Dictionary<int, int>()
            : await _context.MissionMarginSamples
                .AsNoTracking()
                .Where(sample => ids.Contains(sample.SessionId)
                                 && sample.Timestamp >= periodStartUtc
                                 && sample.Timestamp < periodEndUtc)
                .GroupBy(sample => sample.SessionId)
                .Select(group => new { SessionId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(row => row.SessionId, row => row.Count);
        var sampleSummaries = await _missionReportBuilder.GetSampleSummariesAsync(ids, periodStartUtc, periodEndUtc);

        var response = new MissionReportsIndexResponse
        {
            ServerUtc = DateTime.UtcNow,
            Total = total,
            Skip = skip,
            Limit = limit,
            Items = sessions.Select(session =>
            {
                var summary = sampleSummaries.GetValueOrDefault(session.Id);
                return new MissionReportsIndexItem
                {
                    SessionId = session.Id,
                    StartUtc = session.StartTime,
                    EndUtc = session.EndTime,
                    Completed = session.Completed,
                    RuntimeMode = session.RuntimeMode,
                    TotalMarginEuro = session.TotalMargin,
                    PeriodNetPnlEuro = summary?.NetPnl ?? 0m,
                    FinalMarginEuro = summary?.FinalMargin ?? 0m,
                    GlobalTargetEuro = session.GlobalTarget,
                    KFactor = session.KFactor,
                    ActiveTables = session.ActiveTables,
                    RealHandsCount = session.RealHandsCount,
                    SamplesCount = sampleCounts.TryGetValue(session.Id, out var count) ? count : 0
                };
            }).ToList()
        };

        return Ok(ApiResponse<MissionReportsIndexResponse>.SuccessResponse(response));
    }

    [HttpGet("report/{sessionId:int}")]
    public async Task<IActionResult> GetSessionReport([FromRoute] int sessionId, [FromQuery] string? format = "html")
    {
        var report = await _missionReportBuilder.BuildSessionReportAsync(sessionId);
        if (report == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Mission session not found"));

        if (string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
            return Ok(ApiResponse<MissionRangeReportResponse>.SuccessResponse(report));

        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = _missionReportBuilder.BuildCsv(report);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"mission_session_{sessionId}.csv");
        }

        var reportHtml = MissionReportHtmlBuilder.Build(report);
        return File(Encoding.UTF8.GetBytes(reportHtml), "text/html; charset=utf-8", enableRangeProcessing: false);
    }

    [HttpPost("report/{sessionId:int}/resend-email")]
    [Authorize(Policy = AuthConstants.Policies.RequireAdmin)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResendMissionReportEmail([FromRoute] int sessionId, CancellationToken cancellationToken)
    {
        var sent = await _missionLifecycleService.SendMissionEmailAsync(sessionId, "finalized", cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(
            new { sent, sessionId, eventType = "finalized" },
            sent > 0 ? "Mission report email sent" : "No mission notification recipients or email delivery failed"));
    }

    private async Task EnsureMissionReportSchemaAsync()
    {
        await _context.Database.ExecuteSqlRawAsync("""
IF OBJECT_ID(N'[dbo].[MissionSessions]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[MissionSessions] (
        [ID] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_MissionSessions] PRIMARY KEY,
        [MissionKey] nvarchar(128) NULL,
        [StartTime] datetime2 NOT NULL,
        [EndTime] datetime2 NULL,
        [TotalMargin] decimal(18,2) NOT NULL,
        [RealHandsCount] int NOT NULL,
        [LastTotalMarginForRealHands] decimal(18,2) NULL,
        [GlobalTarget] decimal(18,2) NOT NULL,
        [ActiveTables] int NOT NULL,
        [KFactor] decimal(18,2) NOT NULL,
        [RuntimeMode] nvarchar(32) NOT NULL,
        [Completed] bit NOT NULL,
        [ReportPublishedAt] datetime2 NULL,
        [FinalizationReason] nvarchar(128) NULL,
        [CreatedAt] datetime2 NOT NULL
    );
END

IF OBJECT_ID(N'[dbo].[MissionMarginSamples]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[MissionMarginSamples] (
        [ID] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_MissionMarginSamples] PRIMARY KEY,
        [SessionId] int NOT NULL,
        [Timestamp] datetime2 NOT NULL,
        [TotalMargin] decimal(18,2) NOT NULL,
        [ActiveTables] int NOT NULL,
        [VmCurrent] decimal(18,2) NOT NULL,
        [RuntimeMode] nvarchar(32) NOT NULL,
        CONSTRAINT [FK_MissionMarginSamples_MissionSessions_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [dbo].[MissionSessions]([ID]) ON DELETE CASCADE
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MissionSessions_MissionKey' AND object_id = OBJECT_ID(N'[dbo].[MissionSessions]'))
    CREATE UNIQUE INDEX [IX_MissionSessions_MissionKey] ON [dbo].[MissionSessions]([MissionKey]) WHERE [MissionKey] IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MissionSessions_RuntimeMode' AND object_id = OBJECT_ID(N'[dbo].[MissionSessions]'))
    CREATE INDEX [IX_MissionSessions_RuntimeMode] ON [dbo].[MissionSessions]([RuntimeMode]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MissionSessions_StartTime' AND object_id = OBJECT_ID(N'[dbo].[MissionSessions]'))
    CREATE INDEX [IX_MissionSessions_StartTime] ON [dbo].[MissionSessions]([StartTime]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MissionSessions_EndTime' AND object_id = OBJECT_ID(N'[dbo].[MissionSessions]'))
    CREATE INDEX [IX_MissionSessions_EndTime] ON [dbo].[MissionSessions]([EndTime]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MissionSessions_Completed' AND object_id = OBJECT_ID(N'[dbo].[MissionSessions]'))
    CREATE INDEX [IX_MissionSessions_Completed] ON [dbo].[MissionSessions]([Completed]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MissionMarginSamples_SessionId' AND object_id = OBJECT_ID(N'[dbo].[MissionMarginSamples]'))
    CREATE INDEX [IX_MissionMarginSamples_SessionId] ON [dbo].[MissionMarginSamples]([SessionId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MissionMarginSamples_Timestamp' AND object_id = OBJECT_ID(N'[dbo].[MissionMarginSamples]'))
    CREATE INDEX [IX_MissionMarginSamples_Timestamp] ON [dbo].[MissionMarginSamples]([Timestamp]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MissionMarginSamples_RuntimeMode' AND object_id = OBJECT_ID(N'[dbo].[MissionMarginSamples]'))
    CREATE INDEX [IX_MissionMarginSamples_RuntimeMode] ON [dbo].[MissionMarginSamples]([RuntimeMode]);
""");
    }


}

public class FinalizeMissionRequest
{
    public string? Reason { get; set; }
}

public class MissionRangeReportResponse
{
    public string RuntimeMode { get; set; } = "Production";
    public bool IsDemoMode { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public DateTime GeneratedAt { get; set; }
    public MissionRangeTotals Totals { get; set; } = new();
    public MissionQualityMetrics QualityMetrics { get; set; } = new();
    public List<MissionDailyPerformance> DailyRows { get; set; } = new();
    public List<MissionReportSession> Sessions { get; set; } = new();
    public List<MissionReportSample> Samples { get; set; } = new();
}

public class MissionRangeTotals
{
    public decimal PeriodResultEuro { get; set; }
    public decimal TotalMarginEuro { get; set; }
    public decimal FinalMarginEuro { get; set; }
    public decimal GlobalTargetEuro { get; set; }
    public decimal ProgressPct { get; set; }
    public int SampleCount { get; set; }
    public decimal MargineMin { get; set; }
    public decimal MargineMax { get; set; }
    public int SessionCount { get; set; }
    public int RealHandsCount { get; set; }
    public int ActiveTables { get; set; }
    public decimal PeriodReturnPct { get; set; }
    public decimal? AnnualisedReturnPct { get; set; }
    public decimal AverageDailyPnl { get; set; }
    public decimal AverageDailyReturnPct { get; set; }
    public int WorkingDays { get; set; }
    public int ReportingDays { get; set; }
}

public class MissionQualityMetrics
{
    public decimal BestDay { get; set; }
    public decimal WorstDay { get; set; }
    public int PositiveDays { get; set; }
    public decimal WinRatePct { get; set; }
    public decimal MaxDrawdownPct { get; set; }
    public decimal DailyVolatilityPct { get; set; }
    public decimal SharpeRatio { get; set; }
}

public class MissionDailyPerformance
{
    public DateTime Date { get; set; }
    public decimal NetPnl { get; set; }
    public decimal DailyReturnPct { get; set; }
    public decimal CumulativePnl { get; set; }
    public int SampleCount { get; set; }
}

public class MissionReportSample
{
    public int SessionId { get; set; }
    public DateTime DateTime { get; set; }
    public decimal Margine { get; set; }
    public int ActiveTables { get; set; }
}

public class MissionReportSession
{
    public int SessionId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string RuntimeMode { get; set; } = "Production";
    /// <summary>Margine missione ufficiale a chiusura (DB MissionSessions.TotalMargin).</summary>
    public decimal MissionMarginEuro { get; set; }
    public decimal TotalMarginEuro { get; set; }
    public decimal PeriodNetPnlEuro { get; set; }
    public decimal FinalMarginEuro { get; set; }
    public decimal GlobalTargetEuro { get; set; }
    public int ActiveTables { get; set; }
    public int RealHandsCount { get; set; }
}

public class MissionReportsIndexResponse
{
    public DateTime ServerUtc { get; set; }
    public int Total { get; set; }
    public int Skip { get; set; }
    public int Limit { get; set; }
    public List<MissionReportsIndexItem> Items { get; set; } = new();
}

public class MissionReportsIndexItem
{
    public int SessionId { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime? EndUtc { get; set; }
    public bool Completed { get; set; }
    public string RuntimeMode { get; set; } = "Production";
    public decimal TotalMarginEuro { get; set; }
    /// <summary>Delta sample nella finestra contabile (non è il margine missione a chiusura).</summary>
    public decimal PeriodNetPnlEuro { get; set; }
    public decimal FinalMarginEuro { get; set; }
    public decimal GlobalTargetEuro { get; set; }
    public decimal KFactor { get; set; }
    public int ActiveTables { get; set; }
    public int RealHandsCount { get; set; }
    public int SamplesCount { get; set; }
}

public sealed record MissionSampleSummary(decimal NetPnl, decimal FinalMargin);
