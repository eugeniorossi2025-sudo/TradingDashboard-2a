using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
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

    public MissionController(AppDbContext context)
    {
        _context = context;
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

    private async Task<MissionRangeReportResponse> BuildReportAsync(DateTime fromDate, DateTime toDateExclusive, string mode)
    {
        // DASH2A currently stores real margin telemetry in Values without runtime-mode tagging.
        // Production uses existing real data; Demo remains isolated until Demo-tagged history is added.
        var samples = string.Equals(mode, Demo, StringComparison.OrdinalIgnoreCase)
            ? new List<MissionReportSample>()
            : await _context.Values
                .Where(v => v.DateTime >= fromDate && v.DateTime < toDateExclusive && v.Margine.HasValue)
                .OrderBy(v => v.DateTime)
                .Select(v => new MissionReportSample
                {
                    DateTime = v.DateTime,
                    Account = v.Account,
                    Tavolo = v.Tavolo,
                    Margine = v.Margine!.Value,
                    MediaOra = v.MediaOra,
                    Stato = v.Stato
                })
                .ToListAsync();

        var dailyRows = BuildDailyRows(samples, fromDate, toDateExclusive, await GetInvestedCapitalBaseAsync());
        var totalMargin = dailyRows.Sum(row => row.NetPnl);
        var qualityMetrics = BuildQualityMetrics(dailyRows);
        var target = await GetStopWinTargetAsync();
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
                PeriodReturnPct = Math.Round(periodReturnPct, 2),
                AnnualisedReturnPct = Math.Round(annualisedReturnPct, 2),
                AverageDailyPnl = Math.Round(averageDailyPnl, 2),
                AverageDailyReturnPct = Math.Round(averageDailyReturnPct, 2),
                WorkingDays = workingDays,
                ReportingDays = reportingDays
            },
            QualityMetrics = qualityMetrics,
            DailyRows = dailyRows,
            Samples = samples
        };
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
    public DateTime DateTime { get; set; }
    public string? Account { get; set; }
    public int? Tavolo { get; set; }
    public decimal Margine { get; set; }
    public decimal? MediaOra { get; set; }
    public string? Stato { get; set; }
}
