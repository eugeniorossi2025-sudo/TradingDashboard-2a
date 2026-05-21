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
}