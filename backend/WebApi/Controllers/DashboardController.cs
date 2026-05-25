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
    /// Gets only the chart data for dashboard graph (snapshot from current bot statuses).
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

    /// <summary>
    /// Gets the margin time-series from dbo.Margini (written by Decisore).
    /// Returns the last <paramref name="limit"/> points ordered by timestamp.
    /// </summary>
    [HttpGet("margini-chart")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<ChartDataPoint>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMarginiChart([FromQuery] int limit = 200)
    {
        try
        {
            var points = await _dashboardService.GetMarginiChartAsync(limit);
            return Ok(ApiResponse<List<ChartDataPoint>>.SuccessResponse(points));
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                ApiResponse<object>.ErrorResponse($"Error retrieving margini chart: {ex.Message}"));
        }
    }

    /// <summary>
    /// Gets the latest session telemetry from dbo.Statistiche (written by Decisore).
    /// </summary>
    [HttpGet("telemetry")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<DashboardTelemetry>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTelemetry()
    {
        try
        {
            var telemetry = await _dashboardService.GetLatestTelemetryAsync();
            return Ok(ApiResponse<DashboardTelemetry>.SuccessResponse(telemetry));
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                ApiResponse<object>.ErrorResponse($"Error retrieving telemetry: {ex.Message}"));
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
    public string? Computer { get; set; }
    public string? Account { get; set; }
    public string? Tavolo { get; set; }
    public string? Mazzo { get; set; }
    public decimal Margine { get; set; }
    public decimal MediaOra { get; set; }
    public string? ValoreGiocato { get; set; }
    public string? Stato { get; set; }
    public string? Colore { get; set; }
    public string? Pbt { get; set; }
    public string? ColpoMartingala { get; set; }
    public string? Valutazione { get; set; }
    public string? Reason { get; set; }
    public string? LastAdvice { get; set; }
    public string? LastInfo { get; set; }
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
    public DateTime? DtUltimo { get; set; }
    public DateTime? LastUpdate { get; set; }
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

/// <summary>
/// Telemetry from the latest Decisore session (dbo.Statistiche.TELEMETRY deserialized).
/// </summary>
public class DashboardTelemetry
{
    public bool GlobalPauseScalping { get; set; }
    public string GlobalPauseScalpingDetails { get; set; } = "Pausa non attiva";
    public string GlobalPauseScalpingDuration { get; set; } = "0";
    public decimal Inc { get; set; }
    public decimal Ewma { get; set; }
    public int TotalPbHandsPlayed { get; set; }
    public int TotalL5Played { get; set; }
    public int TotalL5Won { get; set; }
    public int TotalL5Lost { get; set; }
    public int SpotId { get; set; }
    public DateTime? SessionStart { get; set; }
    public DateTime? SessionEnd { get; set; }
    public decimal MargineTot { get; set; }
    public decimal MargineMin { get; set; }
    public decimal MargineMax { get; set; }
    public decimal Elapsed { get; set; }
}