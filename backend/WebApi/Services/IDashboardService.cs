using WebApi.Controllers;

namespace WebApi.Services;

/// <summary>
/// Interface for managing dashboard operations.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Gets the latest dashboard statistics asynchronously.
    /// </summary>
    /// <returns>Dashboard statistics.</returns>
    Task<DashboardStatistics> GetDashboardStatisticsAsync();

    /// <summary>
    /// Gets complete dashboard data with table rows and chart data.
    /// </summary>
    /// <returns>Complete dashboard response with tables, chart data, and statistics.</returns>
    Task<DashboardResponse> GetDashboardDataAsync();

    /// <summary>
    /// Gets the margin time-series from dbo.Margini (last <paramref name="limit"/> rows).
    /// </summary>
    Task<List<ChartDataPoint>> GetMarginiChartAsync(int limit = 200);

    /// <summary>
    /// Gets the latest session telemetry from dbo.Statistiche.
    /// </summary>
    Task<DashboardTelemetry> GetLatestTelemetryAsync();
}