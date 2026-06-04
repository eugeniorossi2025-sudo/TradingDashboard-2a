using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Entities;
using WebApi.Controllers;
using WebApi.Data;
using WebApi.Services;

namespace WebApi.Services.Implementations;

public class MissionReportBuilder : IMissionReportBuilder
{
    private const string Production = "Production";
    private const string Demo = "Demo";
    private const string InvestedCapitalBaseKey = "REPORT_INVESTED_CAPITAL_BASE";
    private static readonly TimeZoneInfo RomeTimeZone = ResolveRomeTimeZone();

    private readonly AppDbContext _context;

    public MissionReportBuilder(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MissionRangeReportResponse?> BuildSessionReportAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _context.MissionSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

        if (session == null)
            return null;

        var romeStart = RomeDate(session.StartTime);
        var romeEnd = RomeDate(session.EndTime ?? session.StartTime);
        var fromDate = romeStart.ToDateTime(TimeOnly.MinValue);
        var toDateExclusive = romeEnd.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var report = await BuildRangeReportAsync(fromDate, toDateExclusive, session.RuntimeMode, cancellationToken);
        report.Sessions = report.Sessions.Where(row => row.SessionId == sessionId).ToList();
        report.Samples = report.Samples
            .Where(row => row.SessionId == sessionId)
            .OrderBy(row => row.DateTime)
            .ToList();

        var investedCapitalBase = await GetInvestedCapitalBaseAsync(cancellationToken);
        ApplyCanonicalAccounting(report, fromDate, toDateExclusive, investedCapitalBase);
        return report;
    }

    public async Task<string?> BuildSessionReportHtmlAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        var report = await BuildSessionReportAsync(sessionId, cancellationToken);
        return report == null ? null : MissionReportHtmlBuilder.Build(report);
    }

    public async Task<MissionRangeReportResponse> BuildRangeReportAsync(DateTime fromDate, DateTime toDateExclusive, string mode, CancellationToken cancellationToken = default)
    {
        var (periodStartUtc, periodEndUtc) = GetPeriodBoundsUtc(fromDate, toDateExclusive);
        var candidateSessions = await ApplyAccountingPeriodSessionFilter(
                _context.MissionSessions.AsNoTracking()
                    .Where(s => s.RuntimeMode == mode && s.Completed),
                periodStartUtc,
                periodEndUtc)
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
            .ToListAsync(cancellationToken);

        var candidateIds = candidateSessions.Select(s => s.Id).ToArray();
        var samples = candidateIds.Length == 0
            ? new List<MissionReportSample>()
            : await _context.MissionMarginSamples
                .AsNoTracking()
                .Where(sample => candidateIds.Contains(sample.SessionId)
                                 && sample.Timestamp >= periodStartUtc
                                 && sample.Timestamp < periodEndUtc)
                .OrderBy(sample => sample.Timestamp)
                .Select(sample => new MissionReportSample
                {
                    SessionId = sample.SessionId,
                    DateTime = sample.Timestamp,
                    Margine = sample.TotalMargin,
                    ActiveTables = sample.ActiveTables
                })
                .ToListAsync(cancellationToken);

        // Include every completed session overlapping the period (by UTC bounds).
        // Do not drop missions that lack margin samples clipped inside the window.
        var sessions = candidateSessions;

        var investedCapitalBase = await GetInvestedCapitalBaseAsync();
        var report = new MissionRangeReportResponse
        {
            RuntimeMode = mode,
            IsDemoMode = string.Equals(mode, Demo, StringComparison.OrdinalIgnoreCase),
            From = fromDate,
            To = toDateExclusive.AddDays(-1),
            GeneratedAt = DateTime.UtcNow,
            Sessions = sessions.Select(s => new MissionReportSession
            {
                SessionId = s.Id,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                RuntimeMode = s.RuntimeMode,
                MissionMarginEuro = s.TotalMargin,
                GlobalTargetEuro = s.GlobalTarget,
                ActiveTables = s.ActiveTables,
                RealHandsCount = s.RealHandsCount
            }).ToList(),
            Samples = samples
        };

        ApplyCanonicalAccounting(report, fromDate, toDateExclusive, investedCapitalBase);
        return report;
    }

    private static void ApplyCanonicalAccounting(
        MissionRangeReportResponse report,
        DateTime fromDate,
        DateTime toDateExclusive,
        decimal investedCapitalBase)
    {
        var sampleSummaries = BuildSampleSummaries(report.Samples);
        foreach (var session in report.Sessions)
        {
            var summary = sampleSummaries.GetValueOrDefault(session.SessionId);
            session.PeriodNetPnlEuro = summary?.NetPnl ?? 0m;
            session.FinalMarginEuro = summary?.FinalMargin ?? 0m;
            session.TotalMarginEuro = session.MissionMarginEuro;
        }

        var dailyRows = BuildDailyRows(report.Samples, fromDate, toDateExclusive, investedCapitalBase);
        report.DailyRows = dailyRows;

        var periodResult = Math.Round(sampleSummaries.Values.Sum(row => row.NetPnl), 2);
        var workingDays = dailyRows.Count;
        var reportingDays = Math.Max(1, (toDateExclusive.Date - fromDate.Date).Days);
        var periodReturnPct = investedCapitalBase > 0 ? periodResult / investedCapitalBase * 100 : 0;
        var averageDailyPnl = workingDays > 0 ? periodResult / workingDays : 0;
        var averageDailyReturnPct = investedCapitalBase > 0 ? averageDailyPnl / investedCapitalBase * 100 : 0;
        // Stop Win is per-mission; period header must not sum targets across sessions.
        var target = report.Sessions.Count == 0
            ? 0m
            : report.Sessions.Max(s => s.GlobalTargetEuro);

        var missionMarginSum = Math.Round(report.Sessions.Sum(s => s.MissionMarginEuro), 2);
        report.Totals.PeriodResultEuro = periodResult;
        report.Totals.TotalMarginEuro = missionMarginSum;
        report.Totals.FinalMarginEuro = Math.Round(report.Sessions.Sum(s => s.FinalMarginEuro), 2);
        report.Totals.GlobalTargetEuro = target;
        report.Totals.ProgressPct = target == 0 ? 0 : Math.Round(periodResult / target * 100, 2);
        report.Totals.SampleCount = report.Samples.Count;
        report.Totals.MargineMin = report.Samples.Count == 0 ? 0 : report.Samples.Min(s => s.Margine);
        report.Totals.MargineMax = report.Samples.Count == 0 ? 0 : report.Samples.Max(s => s.Margine);
        report.Totals.SessionCount = report.Sessions.Count;
        report.Totals.RealHandsCount = report.Sessions.Sum(s => s.RealHandsCount);
        report.Totals.ActiveTables = report.Sessions.Count == 0 ? 0 : report.Sessions.Max(s => s.ActiveTables);
        report.Totals.PeriodReturnPct = Math.Round(periodReturnPct, 2);
        report.Totals.AnnualisedReturnPct = CalculateAnnualisedReturnPct(periodReturnPct, workingDays);
        report.Totals.AverageDailyPnl = Math.Round(averageDailyPnl, 2);
        report.Totals.AverageDailyReturnPct = Math.Round(averageDailyReturnPct, 2);
        report.Totals.WorkingDays = workingDays;
        report.Totals.ReportingDays = reportingDays;
        report.QualityMetrics = BuildQualityMetrics(dailyRows, investedCapitalBase);

        EnsureReportCoherence(report);
    }

    private static void EnsureReportCoherence(MissionRangeReportResponse report)
    {
        var periodResult = Math.Round(report.Totals.PeriodResultEuro, 2);
        var sessionSum = Math.Round(report.Sessions.Sum(s => s.PeriodNetPnlEuro), 2);
        var dailySum = Math.Round(report.DailyRows.Sum(r => r.NetPnl), 2);
        var lastCurve = Math.Round(report.DailyRows.LastOrDefault()?.CumulativePnl ?? 0m, 2);

        if (periodResult != sessionSum || periodResult != dailySum || periodResult != lastCurve)
        {
            throw new InvalidOperationException(
                $"Report accounting incoherent: periodResult={periodResult}, sessionSum={sessionSum}, dailySum={dailySum}, lastCurve={lastCurve}");
        }
    }

    public async Task<Dictionary<int, MissionSampleSummary>> GetSampleSummariesAsync(int[] sessionIds, DateTime periodStartUtc, DateTime periodEndUtc, CancellationToken cancellationToken = default)
    {
        if (sessionIds.Length == 0)
            return new Dictionary<int, MissionSampleSummary>();

        var samples = await _context.MissionMarginSamples
            .AsNoTracking()
            .Where(sample => sessionIds.Contains(sample.SessionId)
                             && sample.Timestamp >= periodStartUtc
                             && sample.Timestamp < periodEndUtc)
            .OrderBy(sample => sample.Timestamp)
            .Select(sample => new MissionReportSample
            {
                SessionId = sample.SessionId,
                DateTime = sample.Timestamp,
                Margine = sample.TotalMargin,
                ActiveTables = sample.ActiveTables
            })
            .ToListAsync(cancellationToken);

        return BuildSampleSummaries(samples);
    }

    private static Dictionary<int, MissionSampleSummary> BuildSampleSummaries(IEnumerable<MissionReportSample> samples)
    {
        return samples
            .GroupBy(sample => sample.SessionId)
            .ToDictionary(
                group => group.Key,
                group =>
                {
                    var ordered = group.OrderBy(sample => AsUtc(sample.DateTime)).ToList();
                    if (ordered.Count == 0)
                        return new MissionSampleSummary(0m, 0m);
                    if (ordered.Count == 1)
                        return new MissionSampleSummary(0m, ordered[0].Margine);
                    var first = ordered.First().Margine;
                    var last = ordered.Last().Margine;
                    return new MissionSampleSummary(last - first, last);
                });
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

    private async Task<decimal> GetInvestedCapitalBaseAsync(CancellationToken cancellationToken = default)
    {
        var value = await _context.Configurations
            .Where(c => c.Key == InvestedCapitalBaseKey)
            .Select(c => c.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return decimal.TryParse(value?.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var capital) && capital > 0
            ? capital
            : 5000m;
    }

    private static List<MissionDailyPerformance> BuildDailyRows(
        List<MissionReportSample> samples,
        DateTime fromDate,
        DateTime toDateExclusive,
        decimal investedCapitalBase)
    {
        var dailyTotals = new SortedDictionary<DateOnly, (decimal NetPnl, int SampleCount)>();

        foreach (var sessionGroup in samples.GroupBy(sample => sample.SessionId))
        {
            decimal? previousEndMargin = null;
            foreach (var dayGroup in sessionGroup.GroupBy(sample => RomeDate(sample.DateTime)).OrderBy(group => group.Key))
            {
                var ordered = dayGroup.OrderBy(sample => AsUtc(sample.DateTime)).ToList();
                if (ordered.Count == 0)
                    continue;

                var endMargin = ordered[^1].Margine;
                var startMargin = previousEndMargin ?? ordered[0].Margine;
                var netPnl = endMargin - startMargin;
                previousEndMargin = endMargin;

                if (!dailyTotals.TryGetValue(dayGroup.Key, out var existing))
                    existing = (0m, 0);

                dailyTotals[dayGroup.Key] = (existing.NetPnl + netPnl, existing.SampleCount + ordered.Count);
            }
        }

        var rows = new List<MissionDailyPerformance>();
        decimal cumulative = 0;
        var periodStart = DateOnly.FromDateTime(fromDate.Date);
        var periodEndExclusive = DateOnly.FromDateTime(toDateExclusive.Date);

        for (var day = periodStart; day < periodEndExclusive; day = day.AddDays(1))
        {
            if (!dailyTotals.TryGetValue(day, out var totals))
                continue;

            cumulative += totals.NetPnl;
            rows.Add(new MissionDailyPerformance
            {
                Date = day.ToDateTime(TimeOnly.MinValue),
                NetPnl = Math.Round(totals.NetPnl, 2),
                DailyReturnPct = investedCapitalBase > 0 ? Math.Round(totals.NetPnl / investedCapitalBase * 100, 2) : 0,
                CumulativePnl = Math.Round(cumulative, 2),
                SampleCount = totals.SampleCount
            });
        }

        return rows;
    }

    private static MissionQualityMetrics BuildQualityMetrics(List<MissionDailyPerformance> rows, decimal investedCapitalBase)
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
            MaxDrawdownPct = Math.Round(CalculateMaxDrawdownPct(rows, investedCapitalBase), 2),
            DailyVolatilityPct = Math.Round(volatility, 2),
            SharpeRatio = volatility > 0 ? Math.Round(averageReturn / volatility * (decimal)Math.Sqrt(365), 2) : 0
        };
    }

    private static decimal CalculateMaxDrawdownPct(List<MissionDailyPerformance> rows, decimal investedCapitalBase)
    {
        decimal peakEquity = investedCapitalBase;
        decimal maxDrawdown = 0;

        foreach (var row in rows)
        {
            var equity = investedCapitalBase + row.CumulativePnl;
            peakEquity = Math.Max(peakEquity, equity);
            maxDrawdown = Math.Max(maxDrawdown, peakEquity - equity);
        }

        return peakEquity > 0 ? maxDrawdown / peakEquity * 100 : 0;
    }

    private const int MinimumWorkingDaysForAnnualisedReturn = 7;

    private static decimal? CalculateAnnualisedReturnPct(decimal periodReturnPct, int workingDays)
    {
        if (workingDays < MinimumWorkingDaysForAnnualisedReturn)
            return null;

        var periodReturnDecimal = periodReturnPct / 100m;
        var annualised = (decimal)(Math.Pow((double)(1 + periodReturnDecimal), 365d / workingDays) - 1d) * 100;
        return Math.Round(annualised, 2);
    }

    private static IQueryable<MissionSession> ApplyAccountingPeriodSessionFilter(
        IQueryable<MissionSession> query,
        DateTime periodStartUtc,
        DateTime periodEndUtc)
    {
        return query.Where(session =>
            session.StartTime < periodEndUtc
            && (session.EndTime ?? session.StartTime) >= periodStartUtc);
    }

    public IQueryable<MissionSession> ApplyAccountingPeriodSessionFilterWithSamples(
        IQueryable<MissionSession> query,
        DateTime periodStartUtc,
        DateTime periodEndUtc)
    {
        return ApplyAccountingPeriodSessionFilter(query, periodStartUtc, periodEndUtc)
            .Where(session => _context.MissionMarginSamples.Any(sample =>
                sample.SessionId == session.Id
                && sample.Timestamp >= periodStartUtc
                && sample.Timestamp < periodEndUtc));
    }

    public (DateTime PeriodStartUtc, DateTime PeriodEndUtc) GetPeriodBoundsUtc(DateTime fromDate, DateTime toDateExclusive)
    {
        var startLocal = DateTime.SpecifyKind(fromDate.Date, DateTimeKind.Unspecified);
        var endLocalExclusive = DateTime.SpecifyKind(toDateExclusive.Date, DateTimeKind.Unspecified);
        return (
            TimeZoneInfo.ConvertTimeToUtc(startLocal, RomeTimeZone),
            TimeZoneInfo.ConvertTimeToUtc(endLocalExclusive, RomeTimeZone));
    }

    private static DateOnly RomeDate(DateTime timestamp)
    {
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(AsUtc(timestamp), RomeTimeZone));
    }

    private static DateTime AsUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    private static TimeZoneInfo ResolveRomeTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Rome");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
        }
    }

    public string BuildCsv(MissionRangeReportResponse report)
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


    public string NormalizeMode(string? value)
    {
        return string.Equals(value, Demo, StringComparison.OrdinalIgnoreCase) ? Demo : Production;
    }
}
