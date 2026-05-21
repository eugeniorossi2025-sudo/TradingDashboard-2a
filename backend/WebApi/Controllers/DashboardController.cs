using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers;

/// <summary>
/// Controller for managing dashboard data and real-time updates.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardController"/> class.
    /// </summary>
    /// <param name="dashboardService">The dashboard service.</param>
    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Gets complete dashboard data with table rows, chart data, and statistics.
    /// </summary>
    /// <returns>Complete dashboard response.</returns>
    [HttpGet("data")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<DashboardResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDashboardData()
    {
        try
        {
            var data = await _dashboardService.GetDashboardDataAsync();
            return Ok(ApiResponse<DashboardResponse>.SuccessResponse(data));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse($"Error retrieving dashboard data: {ex.Message}"));
        }
    }

    /// <summary>
    /// Gets real-time updates for dashboard (optimized for frequent polling or WebSocket).
    /// </summary>
    /// <returns>Latest dashboard statistics.</returns>
    [HttpGet("updates")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<DashboardStatistics>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDashboardUpdates()
    {
        try
        {
            var statistics = await _dashboardService.GetDashboardStatisticsAsync();
            return Ok(ApiResponse<DashboardStatistics>.SuccessResponse(statistics));
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                ApiResponse<object>.ErrorResponse($"Error retrieving dashboard updates: {ex.Message}"));
        }
    }

    /// <summary>
    /// Gets only the table rows for active bots.
    /// </summary>
    /// <returns>List of active bot table rows.</returns>
    [HttpGet("tables")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<DashboardTableRow>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardTables()
    {
        try
        {
            var data = await _dashboardService.GetDashboardDataAsync();
            return Ok(ApiResponse<List<DashboardTableRow>>.SuccessResponse(data.Tables));
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                ApiResponse<object>.ErrorResponse($"Error retrieving dashboard tables: {ex.Message}"));
        }
    }

    /// <summary>
    /// Gets only the chart data for dashboard graph.
    /// </summary>
    /// <returns>List of chart data points.</returns>
    [HttpGet("chart")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<ChartDataPoint>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardChart()
    {
        try
        {
            var data = await _dashboardService.GetDashboardDataAsync();
            return Ok(ApiResponse<List<ChartDataPoint>>.SuccessResponse(data.ChartData));
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                ApiResponse<object>.ErrorResponse($"Error retrieving dashboard chart: {ex.Message}"));
        }
    }
}

/// <summary>
/// Represents a complete dashboard response with tables, chart data, and statistics.
/// </summary>
public class DashboardResponse
{
    public required List<DashboardTableRow> Tables { get; set; }
    public required List<ChartDataPoint> ChartData { get; set; }
    public required DashboardStatistics Statistics { get; set; }
}

/// <summary>
/// Represents a single row in the dashboard table.
/// </summary>
public class DashboardTableRow
{
    public string? MinutiPassati { get; set; }
    public string? Account { get; set; }
    public string? Tavolo { get; set; }
    public string? Mazzo { get; set; }
    public decimal Margine { get; set; }
    public decimal MediaOra { get; set; }
    public string? ValoreGiocato { get; set; }
    public string? Stato { get; set; }
    public string? Colore { get; set; }
    public string? ColpoMartingala { get; set; }
    public string? Valutazione { get; set; }
    public string? Reason { get; set; }
    public string? Prediction { get; set; }
    public string? FutureL5Pred { get; set; }
    public string? StopAtL5 { get; set; }
    public string? AuthorizerHeavy { get; set; }
    public string? TableScore { get; set; }
    public string? LevelIndex { get; set; }
    public string? StakeUnit { get; set; }
    public string? HotZone { get; set; }
    public string? VmLocal20 { get; set; }
    public decimal SaldoIniziale { get; set; }
    public decimal SaldoIstantaneo { get; set; }
    public string? Ore { get; set; }
    public string? Note { get; set; }
    public string? Json { get; set; }
}

/// <summary>
/// Represents a data point for the chart.
/// </summary>
public class ChartDataPoint
{
    public DateTime DateTime { get; set; }
    public decimal Margine { get; set; }
}

/// <summary>
/// Represents dashboard statistics and aggregated data.
/// </summary>
public class DashboardStatistics
{
    public decimal TotalMargine { get; set; }
    public string? TempoTrascorso { get; set; }
    public decimal MargineMin { get; set; }
    public decimal MargineMax { get; set; }
    public decimal MargineAttuale { get; set; }
    public int TotaleRighe { get; set; }
}