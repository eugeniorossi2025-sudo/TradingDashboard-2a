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

    /// <summary>
    /// Full Security Filter telemetry for one bot (proxied from Decisore in-memory state).
    /// </summary>
    [HttpGet("security-filter/{computer}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<SecurityFilterBotTelemetryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSecurityFilterBot(string computer, CancellationToken cancellationToken)
    {
        try
        {
            var detail = await _dashboardService.GetSecurityFilterBotDetailAsync(computer, cancellationToken);
            if (detail == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"Security filter detail not available for '{computer}'."));
            }

            return Ok(ApiResponse<SecurityFilterBotTelemetryDto>.SuccessResponse(detail));
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                ApiResponse<object>.ErrorResponse($"Error retrieving security filter detail: {ex.Message}"));
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
    public string? ChosenColor { get; set; }
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
    // Decisore lastAdvice — typed extracts
    public bool? AdviceStopL6 { get; set; }
    public int? AdviceGlobalL5Loss { get; set; }
    public int? AdviceGlobalAuthL6Counter { get; set; }
    public int? AdviceActionCode { get; set; }
    public int? AdviceMartingala { get; set; }
    public bool? AdviceHotZone { get; set; }
    public string? AdviceHotZoneLabel { get; set; }
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
    public int TotalL8Played { get; set; }
    public int TotalL8Won { get; set; }
    public int TotalL8Lost { get; set; }
    public int TotalAuthL6Authorized { get; set; }
    public int TotalPauseScalpingSoglieActivated { get; set; }
    public int TotalPauseScalpingEWMAActivated { get; set; }
    public int SpotId { get; set; }
    public int SpotPbHandsPlayed { get; set; }
    public int SpotAuthL6Counter { get; set; }
    public int SpotL5Loss { get; set; }
    public bool SecurityFilterEnabled { get; set; } = true;
    public int SecurityFilterMinScore { get; set; } = 3;
    public int SecurityFilterMinStreak { get; set; } = 5;
    public int SecurityFilterMaxShoeHand { get; set; } = 20;
    public decimal SecurityFilterMaxAvgSeconds { get; set; } = 23.5m;
    public decimal SecurityFilterVeryFastSeconds { get; set; } = 21.0m;
    public int SecurityFilterDeltaWindow { get; set; } = 8;
    public int TotalSecurityFilterActivated { get; set; }
    public int TotalSecurityFilterPreventedL6 { get; set; }
    public decimal LastAvgHandSeconds { get; set; }
    public int ActiveSecurityFilterBots { get; set; }
    public Dictionary<string, SecurityFilterBotTelemetryDto> SecurityFilterByBot { get; set; } = new();
    public DateTime? SessionStart { get; set; }
    public DateTime? SessionEnd { get; set; }
    public decimal MargineTot { get; set; }
    public decimal MargineMin { get; set; }
    public decimal MargineMax { get; set; }
    public decimal Elapsed { get; set; }
    /// <summary>Raw TELEMETRY JSON string from dbo.Statistiche — passed through to the frontend as-is.</summary>
    public string? RawTelemetry { get; set; }
}

public class SecurityFilterBotTelemetryDto
{
    public string Computer { get; set; } = "";
    public decimal AvgHandSeconds { get; set; }
    public decimal LastHandDeltaSeconds { get; set; }
    public double[] LastTwoHandDeltaSeconds { get; set; } = Array.Empty<double>();
    public bool RapidL5TriggerActive { get; set; }
    public decimal MinHandDeltaSeconds { get; set; }
    public decimal MaxHandDeltaSeconds { get; set; }
    public int L6PlayedCount { get; set; }
    public decimal LastL6DeltaSeconds { get; set; }
    public decimal AvgL6DeltaSeconds { get; set; }
    public decimal MinL6DeltaSeconds { get; set; }
    public decimal MaxL6DeltaSeconds { get; set; }
    public int LastL6DeltaHands { get; set; }
    public decimal AvgL6DeltaHands { get; set; }
    public int MinL6DeltaHands { get; set; }
    public int MaxL6DeltaHands { get; set; }
    public int L6DeltaSamples { get; set; }
    public DateTime LastL6PlayedAtUtc { get; set; }
    public int LastL6PlayedPBHands { get; set; }
    public int AuthorizedL8LostCount { get; set; }
    public decimal LastAuthorizedL8LostDeltaSeconds { get; set; }
    public decimal AvgAuthorizedL8LostDeltaSeconds { get; set; }
    public decimal MinAuthorizedL8LostDeltaSeconds { get; set; }
    public decimal MaxAuthorizedL8LostDeltaSeconds { get; set; }
    public int LastAuthorizedL8LostDeltaHands { get; set; }
    public decimal AvgAuthorizedL8LostDeltaHands { get; set; }
    public int MinAuthorizedL8LostDeltaHands { get; set; }
    public int MaxAuthorizedL8LostDeltaHands { get; set; }
    public int AuthorizedL8LostDeltaSamples { get; set; }
    public DateTime LastAuthorizedL8LostAtUtc { get; set; }
    public int LastAuthorizedL8LostPBHands { get; set; }
    public DateTime LastL6AuthorizationAtUtc { get; set; }
    public int PBHandsPlayed { get; set; }
    public int LastL6AuthorizationPBHandsPlayed { get; set; }
    public int LastL6AuthorizationScore { get; set; }
    public int LastL6AuthorizationStreak { get; set; }
    public int LastL6AuthorizationShoeHand { get; set; }
    public decimal LastL6AuthorizationAvgHandSeconds { get; set; }
    public int AuthorizedL8LostFromAuthorizationCount { get; set; }
    public decimal LastAuthorizedL8LossFromAuthorizationSeconds { get; set; }
    public decimal AvgAuthorizedL8LossFromAuthorizationSeconds { get; set; }
    public decimal MinAuthorizedL8LossFromAuthorizationSeconds { get; set; }
    public decimal MaxAuthorizedL8LossFromAuthorizationSeconds { get; set; }
    public int LastAuthorizedL8LossFromAuthorizationHands { get; set; }
    public decimal LastAuthorizedL8LossSecondsPerHand { get; set; }
    public decimal AvgAuthorizedL8LossSecondsPerHand { get; set; }
    public decimal MinAuthorizedL8LossSecondsPerHand { get; set; }
    public decimal MaxAuthorizedL8LossSecondsPerHand { get; set; }
    public int LastAuthorizedL8LossAuthorizationScore { get; set; }
    public decimal AvgAuthorizedL8LossAuthorizationScore { get; set; }
    public int CurrentStreak { get; set; }
    public int SecurityRiskScore { get; set; }
    public bool SecurityFilterActive { get; set; }
    public bool PauseBot { get; set; }
    public string PauseScope { get; set; } = "NONE";
    public string PauseComputer { get; set; } = "";
    public int Activations { get; set; }
    public int PreventedL6 { get; set; }
    public int LastShoeHand { get; set; }
    public int Martingala { get; set; }
    public bool HasL6Credit { get; set; }
    public string LastReason { get; set; } = "";
    public DateTime LastUpdatedUtc { get; set; }
    public int HandSamples { get; set; }
    public int ValidSamples { get; set; }
    public int GapFilteredCount { get; set; }
}

/// <summary>
/// Fields extracted from the Decisore lastAdvice JSON for a single bot row.
/// </summary>
public class LastAdviceFields
{
    public bool? StopL6 { get; set; }
    public int? GlobalL5Loss { get; set; }
    public int? GlobalAuthL6Counter { get; set; }
    public int? ActionCode { get; set; }
    public int? Martingala { get; set; }
    public bool? HotZone { get; set; }
    public string? HotZoneLabel { get; set; }
    public string? Reason { get; set; }
}