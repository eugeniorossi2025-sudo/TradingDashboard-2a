using WebApi.Controllers;

namespace WebApi.Services;

public interface IMissionReportBuilder
{
    Task<MissionRangeReportResponse?> BuildSessionReportAsync(int sessionId, CancellationToken cancellationToken = default);
    Task<string?> BuildSessionReportHtmlAsync(int sessionId, CancellationToken cancellationToken = default);
    Task<MissionRangeReportResponse> BuildRangeReportAsync(DateTime fromDate, DateTime toDateExclusive, string mode, CancellationToken cancellationToken = default);
    Task<Dictionary<int, MissionSampleSummary>> GetSampleSummariesAsync(int[] sessionIds, DateTime periodStartUtc, DateTime periodEndUtc, CancellationToken cancellationToken = default);
    IQueryable<Entities.MissionSession> ApplyAccountingPeriodSessionFilterWithSamples(IQueryable<Entities.MissionSession> query, DateTime periodStartUtc, DateTime periodEndUtc);
    (DateTime PeriodStartUtc, DateTime PeriodEndUtc) GetPeriodBoundsUtc(DateTime fromDate, DateTime toDateExclusive);
    string NormalizeMode(string? value);
    string BuildCsv(MissionRangeReportResponse report);
}
