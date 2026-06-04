using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApi.Options;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers;

/// <summary>
/// Exposes Decider configuration and reachability probes only.
/// Not part of the dashboard data pipeline; no Decider-to-local-DB sync.
/// </summary>
[ApiController]
[Route("api/decider")]
[Produces("application/json")]
[Authorize]
public class DeciderController : ControllerBase
{
    public const string ResetIncompleteUserMessage = "RESET NON COMPLETATO — MISSIONI ANCORA BLOCCATE";

    private readonly DeciderOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMissionLifecycleService _missionLifecycleService;
    private readonly ILogger<DeciderController> _logger;

    public DeciderController(
        IOptions<DeciderOptions> options,
        IHttpClientFactory httpClientFactory,
        IMissionLifecycleService missionLifecycleService,
        ILogger<DeciderController> logger)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _missionLifecycleService = missionLifecycleService;
        _logger = logger;
    }

    [HttpGet("config")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult GetConfig()
    {
        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            enabled = _options.Enabled,
            mode = _options.Mode,
            baseUrl = _options.ApiBaseUrl,
            apiBasePath = _options.ApiBasePath,
            proactiveResetUrl = _options.ProactiveUrl("reset"),
        }));
    }

    [HttpPost("reset")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> TriggerReset(CancellationToken cancellationToken)
    {
        var url = _options.ProactiveUrl("reset");
        var client = _httpClientFactory.CreateClient(nameof(DeciderController));
        client.Timeout = TimeSpan.FromSeconds(15);

        _logger.LogInformation("RESET DASHBOARD START url={ResetUrl}", url);

        try
        {
            await _missionLifecycleService.RecoverMultipleOpenSessionsAsync(cancellationToken);

            var missionResult = await _missionLifecycleService.FinalizeCurrentAsync("ResetDashboard", cancellationToken);
            if (missionResult.MissionFinalized)
            {
                _logger.LogInformation(
                    "MISSIONE CHIUSA sessionId={SessionId} reason=ResetDashboard totalMargin={TotalMargin:0.00}",
                    missionResult.MissionSessionId,
                    missionResult.Mission?.TotalMargin);
            }
            else
            {
                _logger.LogInformation("MISSIONE CHIUSA nessuna missione aperta da finalizzare");
            }

            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync(url, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DECISORE RESET KO url={ResetUrl} unreachable", url);
                return IncompleteReset(missionResult);
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "DECISORE RESET KO url={ResetUrl} statusCode={StatusCode}",
                    url,
                    (int)response.StatusCode);
                return IncompleteReset(missionResult);
            }

            _logger.LogInformation("DECISORE RESET OK url={ResetUrl} statusCode={StatusCode}", url, (int)response.StatusCode);

            await _missionLifecycleService.RecordResetBoundaryAsync(cancellationToken);
            _logger.LogInformation("RESET BOUNDARY WRITTEN");

            var boundary = await _missionLifecycleService.GetResetBoundaryStateAsync(cancellationToken);
            if (!IsResetBoundaryVerified(boundary, out var verifyDetail))
            {
                _logger.LogWarning(
                    "RESET BOUNDARY VERIFY FAILED suppress={Suppress} suppressed={Suppressed} lastResetUtc={LastResetUtc} detail={Detail}",
                    boundary.MissionSuppressStartUntilReset,
                    boundary.MissionStartSuppressed,
                    boundary.MissionLastResetAtUtc,
                    verifyDetail);
                return IncompleteReset(missionResult);
            }

            _logger.LogInformation(
                "RESET DASHBOARD COMPLETE suppress={Suppress} lastResetUtc={LastResetUtc:o}",
                boundary.MissionSuppressStartUntilReset,
                boundary.MissionLastResetAtUtc);

            return Ok(ApiResponse<object>.SuccessResponse(
                new
                {
                    url,
                    mission = missionResult,
                    resetBoundary = new
                    {
                        boundary.MissionSuppressStartUntilReset,
                        boundary.MissionLastResetAtUtc
                    }
                },
                "Reset dashboard completato"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RESET DASHBOARD FAILED url={ResetUrl}", url);
            return StatusCode(502, ApiResponse<object>.ErrorResponse(ResetIncompleteUserMessage));
        }
    }

    private static bool IsResetBoundaryVerified(MissionResetBoundaryState boundary, out string detail)
    {
        if (boundary.MissionStartSuppressed)
        {
            detail = $"MISSION_SUPPRESS_START_UNTIL_RESET={boundary.MissionSuppressStartUntilReset ?? "(null)"}";
            return false;
        }

        if (!boundary.MissionLastResetAtUtc.HasValue)
        {
            detail = "MISSION_LAST_RESET_AT_UTC missing";
            return false;
        }

        detail = string.Empty;
        return true;
    }

    private IActionResult IncompleteReset(MissionLifecycleResult missionResult)
    {
        return StatusCode(502, ApiResponse<object>.ErrorResponse(
            ResetIncompleteUserMessage,
            new List<string>
            {
                missionResult.MissionFinalized
                    ? $"Missione #{missionResult.MissionSessionId} finalizzata ma il reset operatore non e' completo."
                    : "Reset operatore non completo; le missioni restano bloccate fino a un reset riuscito."
            }));
    }

    [HttpPost("emergency-stop")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> TriggerEmergencyStop(CancellationToken cancellationToken)
    {
        var url = _options.ProactiveUrl("emergency-stop");
        var client = _httpClientFactory.CreateClient(nameof(DeciderController));
        client.Timeout = TimeSpan.FromSeconds(15);
        try
        {
            var response = await client.GetAsync(url, cancellationToken);
            return response.IsSuccessStatusCode
                ? Ok(ApiResponse<object>.SuccessResponse(new { url }, "Emergency stop inviato al Decisore"))
                : StatusCode(502, ApiResponse<object>.ErrorResponse($"Decisore ha risposto {(int)response.StatusCode}"));
        }
        catch (Exception ex)
        {
            return StatusCode(502, ApiResponse<object>.ErrorResponse($"Decisore non raggiungibile: {ex.Message}"));
        }
    }

    [HttpGet("health")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Health(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                enabled = false,
                reachable = false,
                statusCode = 0,
                url = _options.ApiBaseUrl,
                message = "Decider integration disabled in configuration.",
            }));
        }

        var probeUrl = _options.ApiBaseUrl;
        var client = _httpClientFactory.CreateClient(nameof(DeciderController));
        client.Timeout = TimeSpan.FromSeconds(10);

        try
        {
            var response = await client.GetAsync(probeUrl, cancellationToken);
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                enabled = true,
                reachable = true,
                statusCode = (int)response.StatusCode,
                url = probeUrl,
            }));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                enabled = true,
                reachable = false,
                statusCode = 0,
                url = probeUrl,
                error = ex.Message,
            }));
        }
    }
}
