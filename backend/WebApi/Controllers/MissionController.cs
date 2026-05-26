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
    private const string InvestedCapitalBaseKey = "REPORT_INVESTED_CAPITAL_BASE";

    private readonly AppDbContext _context;
    private readonly IMissionLifecycleService _missionLifecycleService;

    public MissionController(AppDbContext context, IMissionLifecycleService missionLifecycleService)
    {
        _context = context;
        _missionLifecycleService = missionLifecycleService;
    }

    [HttpGet("current")]
    [ProducesResponseType(typeof(ApiResponse<MissionLifecycleState>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
    {
        var current = await _missionLifecycleService.GetCurrentAsync(cancellationToken);
        return Ok(ApiResponse<MissionLifecycleState>.SuccessResponse(current));
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
        var mode = NormalizeMode(runtimeMode);
        var fromDate = (from ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
        var toDateExclusive = (to ?? DateTime.Today).Date.AddDays(1);

        await EnsureMissionReportSchemaAsync();
        var report = await BuildReportAsync(fromDate, toDateExclusive, mode);

        if (string.Equals(format, "html", StringComparison.OrdinalIgnoreCase))
        {
            var html = MissionReportHtmlBuilder.Build(report);
            return File(Encoding.UTF8.GetBytes(html), "text/html; charset=utf-8", enableRangeProcessing: false);
        }

        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = BuildCsv(report);
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
        var mode = NormalizeMode(runtimeMode);
        var includeAllModes = string.Equals(runtimeMode, "All", StringComparison.OrdinalIgnoreCase);
        var fromDate = (fromUtc ?? DateTime.UtcNow.AddYears(-10)).Date;
        var toExclusive = (toUtc ?? DateTime.UtcNow).Date.AddDays(1);
        await EnsureMissionReportSchemaAsync();

        limit = Math.Clamp(limit, 1, 500);
        skip = Math.Max(0, skip);

        var query = _context.MissionSessions
            .AsNoTracking()
            .Where(session => (session.EndTime ?? session.StartTime) >= fromDate
                              && session.StartTime < toExclusive);

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
                .Where(sample => ids.Contains(sample.SessionId))
                .GroupBy(sample => sample.SessionId)
                .Select(group => new { SessionId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(row => row.SessionId, row => row.Count);

        var response = new MissionReportsIndexResponse
        {
            ServerUtc = DateTime.UtcNow,
            Total = total,
            Skip = skip,
            Limit = limit,
            Items = sessions.Select(session => new MissionReportsIndexItem
            {
                SessionId = session.Id,
                StartUtc = session.StartTime,
                EndUtc = session.EndTime,
                Completed = session.Completed,
                RuntimeMode = session.RuntimeMode,
                TotalMarginEuro = session.TotalMargin,
                GlobalTargetEuro = session.GlobalTarget,
                KFactor = session.KFactor,
                ActiveTables = session.ActiveTables,
                RealHandsCount = session.RealHandsCount,
                SamplesCount = sampleCounts.TryGetValue(session.Id, out var count) ? count : 0
            }).ToList()
        };

        return Ok(ApiResponse<MissionReportsIndexResponse>.SuccessResponse(response));
    }

    [HttpGet("report/{sessionId:int}")]
    public async Task<IActionResult> GetSessionReport([FromRoute] int sessionId, [FromQuery] string? format = "html")
    {
        var session = await _context.MissionSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Mission session not found"));

        var report = await BuildReportAsync(session.StartTime.Date, (session.EndTime ?? session.StartTime).Date.AddDays(1), session.RuntimeMode);
        report.Sessions = report.Sessions
            .Where(row => row.SessionId == sessionId)
            .ToList();
        report.Samples = report.Samples
            .Where(row => row.SessionId == sessionId)
            .OrderBy(row => row.DateTime)
            .ToList();
        report.DailyRows = BuildDailyRows(report.Samples, session.StartTime.Date, (session.EndTime ?? session.StartTime).Date.AddDays(1), await GetInvestedCapitalBaseAsync());
        var investedCapitalBase = await GetInvestedCapitalBaseAsync();
        var reportingDays = Math.Max(1, ((session.EndTime ?? session.StartTime).Date.AddDays(1) - session.StartTime.Date).Days);
        var workingDays = report.DailyRows.Count(row => row.SampleCount > 0);
        report.Totals.TotalMarginEuro = report.DailyRows.Sum(row => row.NetPnl);
        report.Totals.GlobalTargetEuro = report.Sessions.Sum(row => row.GlobalTargetEuro);
        report.Totals.ProgressPct = report.Totals.GlobalTargetEuro == 0 ? 0 : Math.Round(report.Totals.TotalMarginEuro / report.Totals.GlobalTargetEuro * 100, 2);
        report.Totals.SampleCount = report.Samples.Count;
        report.Totals.MargineMin = report.Samples.Count == 0 ? 0 : report.Samples.Min(row => row.Margine);
        report.Totals.MargineMax = report.Samples.Count == 0 ? 0 : report.Samples.Max(row => row.Margine);
        report.Totals.SessionCount = report.Sessions.Count;
        report.Totals.RealHandsCount = report.Sessions.Sum(row => row.RealHandsCount);
        report.Totals.ActiveTables = report.Sessions.Count == 0 ? 0 : report.Sessions.Max(row => row.ActiveTables);
        report.Totals.ReportingDays = reportingDays;
        report.Totals.WorkingDays = workingDays;
        report.Totals.PeriodReturnPct = investedCapitalBase > 0 ? Math.Round(report.Totals.TotalMarginEuro / investedCapitalBase * 100, 2) : 0;
        report.Totals.AverageDailyPnl = workingDays > 0 ? Math.Round(report.Totals.TotalMarginEuro / workingDays, 2) : 0;
        report.Totals.AverageDailyReturnPct = investedCapitalBase > 0 ? Math.Round(report.Totals.AverageDailyPnl / investedCapitalBase * 100, 2) : 0;
        report.Totals.AnnualisedReturnPct = reportingDays > 0 ? Math.Round(report.Totals.PeriodReturnPct / reportingDays * 365, 2) : 0;
        report.QualityMetrics = BuildQualityMetrics(report.DailyRows);

        if (string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
            return Ok(ApiResponse<MissionRangeReportResponse>.SuccessResponse(report));

        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = BuildCsv(report);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"mission_session_{sessionId}.csv");
        }

        var html = MissionReportHtmlBuilder.Build(report);
        return File(Encoding.UTF8.GetBytes(html), "text/html; charset=utf-8", enableRangeProcessing: false);
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

    private async Task<MissionRangeReportResponse> BuildReportAsync(DateTime fromDate, DateTime toDateExclusive, string mode)
    {
        var sessions = await _context.MissionSessions
            .AsNoTracking()
            .Where(s => s.RuntimeMode == mode
                        && s.Completed
                        && (s.EndTime ?? s.StartTime) >= fromDate
                        && s.StartTime < toDateExclusive)
            .OrderBy(s => s.StartTime)
            .ThenBy(s => s.Id)
            .Select(s => new
            {
                s.Id,
                s.StartTime,
                s.EndTime,
                s.TotalMargin,
                s.GlobalTarget,
                s.ActiveTables,
                s.RealHandsCount,
                s.RuntimeMode
            })
            .ToListAsync();

        var sessionIds = sessions.Select(s => s.Id).ToArray();
        var samples = sessionIds.Length == 0
            ? new List<MissionReportSample>()
            : await _context.MissionMarginSamples
                .AsNoTracking()
                .Where(sample => sessionIds.Contains(sample.SessionId))
                .OrderBy(sample => sample.Timestamp)
                .Select(sample => new MissionReportSample
                {
                    SessionId = sample.SessionId,
                    DateTime = sample.Timestamp,
                    Margine = sample.TotalMargin,
                    ActiveTables = sample.ActiveTables
                })
                .ToListAsync();

        var dailyRows = BuildDailyRows(samples, fromDate, toDateExclusive, await GetInvestedCapitalBaseAsync());
        var totalMargin = dailyRows.Sum(row => row.NetPnl);
        var qualityMetrics = BuildQualityMetrics(dailyRows);
        var target = sessions.Sum(s => s.GlobalTarget);
        var investedCapitalBase = await GetInvestedCapitalBaseAsync();
        var reportingDays = Math.Max(1, (toDateExclusive.Date - fromDate.Date).Days);
        var workingDays = dailyRows.Count(row => row.SampleCount > 0);
        var periodReturnPct = investedCapitalBase > 0 ? totalMargin / investedCapitalBase * 100 : 0;
        var averageDailyPnl = workingDays > 0 ? totalMargin / workingDays : 0;
        var averageDailyReturnPct = investedCapitalBase > 0 ? averageDailyPnl / investedCapitalBase * 100 : 0;
        var annualisedReturnPct = reportingDays > 0 ? periodReturnPct / reportingDays * 365 : 0;

        return new MissionRangeReportResponse
        {
            RuntimeMode = mode,
            IsDemoMode = string.Equals(mode, Demo, StringComparison.OrdinalIgnoreCase),
            From = fromDate,
            To = toDateExclusive.AddDays(-1),
            GeneratedAt = DateTime.UtcNow,
            Totals = new MissionRangeTotals
            {
                TotalMarginEuro = totalMargin,
                GlobalTargetEuro = target,
                ProgressPct = target == 0 ? 0 : Math.Round(totalMargin / target * 100, 2),
                SampleCount = samples.Count,
                MargineMin = samples.Count == 0 ? 0 : samples.Min(s => s.Margine),
                MargineMax = samples.Count == 0 ? 0 : samples.Max(s => s.Margine),
                SessionCount = sessions.Count,
                RealHandsCount = sessions.Sum(s => s.RealHandsCount),
                ActiveTables = sessions.Count == 0 ? 0 : sessions.Max(s => s.ActiveTables),
                PeriodReturnPct = Math.Round(periodReturnPct, 2),
                AnnualisedReturnPct = Math.Round(annualisedReturnPct, 2),
                AverageDailyPnl = Math.Round(averageDailyPnl, 2),
                AverageDailyReturnPct = Math.Round(averageDailyReturnPct, 2),
                WorkingDays = workingDays,
                ReportingDays = reportingDays
            },
            QualityMetrics = qualityMetrics,
            DailyRows = dailyRows,
            Sessions = sessions.Select(s => new MissionReportSession
            {
                SessionId = s.Id,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                RuntimeMode = s.RuntimeMode,
                TotalMarginEuro = s.TotalMargin,
                GlobalTargetEuro = s.GlobalTarget,
                ActiveTables = s.ActiveTables,
                RealHandsCount = s.RealHandsCount
            }).ToList(),
            Samples = samples
        };
    }

    private static int CountMarginMoves(IEnumerable<decimal> margins)
    {
        decimal? previous = null;
        var count = 0;
        foreach (var margin in margins)
        {
            if (previous.HasValue && margin != previous.Value)
                count++;
            previous = margin;
        }
        return count;
    }

    private async Task<decimal> GetStopWinTargetAsync()
    {
        var value = await _context.Configurations
            .Where(c => c.Key == "STOP_WIN")
            .Select(c => c.Value)
            .FirstOrDefaultAsync();

        return decimal.TryParse(value?.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var target)
            ? target
            : 0;
    }

    private async Task<decimal> GetInvestedCapitalBaseAsync()
    {
        var value = await _context.Configurations
            .Where(c => c.Key == InvestedCapitalBaseKey)
            .Select(c => c.Value)
            .FirstOrDefaultAsync();

        return decimal.TryParse(value?.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var capital) && capital > 0
            ? capital
            : 5000m;
    }

    private static List<MissionDailyPerformance> BuildDailyRows(List<MissionReportSample> samples, DateTime fromDate, DateTime toDateExclusive, decimal investedCapitalBase)
    {
        var rows = new List<MissionDailyPerformance>();
        decimal cumulative = 0;

        foreach (var group in samples.GroupBy(sample => sample.DateTime.Date).OrderBy(group => group.Key))
        {
            var ordered = group.OrderBy(sample => sample.DateTime).ToList();
            if (ordered.Count == 0) continue;

            var first = ordered.First().Margine;
            var last = ordered.Last().Margine;
            var netPnl = last - first;
            cumulative += netPnl;

            rows.Add(new MissionDailyPerformance
            {
                Date = group.Key,
                NetPnl = Math.Round(netPnl, 2),
                DailyReturnPct = investedCapitalBase > 0 ? Math.Round(netPnl / investedCapitalBase * 100, 2) : 0,
                CumulativePnl = Math.Round(cumulative, 2),
                SampleCount = ordered.Count
            });
        }

        return rows;
    }

    private static MissionQualityMetrics BuildQualityMetrics(List<MissionDailyPerformance> rows)
    {
        if (rows.Count == 0)
        {
            return new MissionQualityMetrics();
        }

        var pnls = rows.Select(row => row.NetPnl).ToList();
        var returns = rows.Select(row => row.DailyReturnPct).ToList();
        var averageReturn = returns.Average();
        var volatility = returns.Count > 1
            ? (decimal)Math.Sqrt(returns.Average(value => Math.Pow((double)(value - averageReturn), 2)))
            : 0m;

        return new MissionQualityMetrics
        {
            BestDay = pnls.Max(),
            WorstDay = pnls.Min(),
            PositiveDays = pnls.Count(value => value > 0),
            WinRatePct = Math.Round(pnls.Count(value => value > 0) / (decimal)pnls.Count * 100, 2),
            MaxDrawdownPct = Math.Round(CalculateMaxDrawdownPct(rows), 2),
            DailyVolatilityPct = Math.Round(volatility, 2),
            SharpeRatio = volatility > 0 ? Math.Round(averageReturn / volatility * (decimal)Math.Sqrt(365), 2) : 0
        };
    }

    private static decimal CalculateMaxDrawdownPct(List<MissionDailyPerformance> rows)
    {
        decimal peak = 0;
        decimal maxDrawdown = 0;

        foreach (var row in rows)
        {
            peak = Math.Max(peak, row.CumulativePnl);
            var drawdown = peak - row.CumulativePnl;
            maxDrawdown = Math.Max(maxDrawdown, drawdown);
        }

        var basis = rows.Select(row => Math.Abs(row.CumulativePnl)).DefaultIfEmpty(0).Max();
        return basis > 0 ? maxDrawdown / basis * 100 : 0;
    }

    private static string BuildCsv(MissionRangeReportResponse report)
    {
        var rows = new List<string>
        {
            "RuntimeMode,Date,NetPnL,DailyReturnPct,CumulativePnL,SampleCount"
        };

        rows.AddRange(report.DailyRows.Select(row => string.Join(",",
            Escape(report.RuntimeMode),
            Escape(row.Date.ToString("yyyy-MM-dd")),
            Escape(row.NetPnl.ToString(CultureInfo.InvariantCulture)),
            Escape(row.DailyReturnPct.ToString(CultureInfo.InvariantCulture)),
            Escape(row.CumulativePnl.ToString(CultureInfo.InvariantCulture)),
            Escape(row.SampleCount.ToString(CultureInfo.InvariantCulture)))));

        return string.Join("\n", rows);
    }

    private static string Escape(string? value)
    {
        return $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";
    }

    private static string NormalizeMode(string? value)
    {
        return string.Equals(value, Demo, StringComparison.OrdinalIgnoreCase) ? Demo : Production;
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
    public decimal TotalMarginEuro { get; set; }
    public decimal GlobalTargetEuro { get; set; }
    public decimal ProgressPct { get; set; }
    public int SampleCount { get; set; }
    public decimal MargineMin { get; set; }
    public decimal MargineMax { get; set; }
    public int SessionCount { get; set; }
    public int RealHandsCount { get; set; }
    public int ActiveTables { get; set; }
    public decimal PeriodReturnPct { get; set; }
    public decimal AnnualisedReturnPct { get; set; }
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
    public decimal TotalMarginEuro { get; set; }
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
    public decimal GlobalTargetEuro { get; set; }
    public decimal KFactor { get; set; }
    public int ActiveTables { get; set; }
    public int RealHandsCount { get; set; }
    public int SamplesCount { get; set; }
}
