using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApi.Options;
using WebApi.Models;

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
    private readonly DeciderOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;

    public DeciderController(IOptions<DeciderOptions> options, IHttpClientFactory httpClientFactory)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
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
        try
        {
            var response = await client.GetAsync(url, cancellationToken);
            return response.IsSuccessStatusCode
                ? Ok(ApiResponse<object>.SuccessResponse(new { url }, "Reset inviato al Decisore"))
                : StatusCode(502, ApiResponse<object>.ErrorResponse($"Decisore ha risposto {(int)response.StatusCode}"));
        }
        catch (Exception ex)
        {
            return StatusCode(502, ApiResponse<object>.ErrorResponse($"Decisore non raggiungibile: {ex.Message}"));
        }
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
                url = _options.ProactiveUrl("reset"),
                message = "Decider integration disabled in configuration.",
            }));
        }

        var probeUrl = _options.ProactiveUrl("reset");
        var client = _httpClientFactory.CreateClient(nameof(DeciderController));
        client.Timeout = TimeSpan.FromSeconds(10);

        try
        {
            var response = await client.GetAsync(probeUrl, cancellationToken);
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                enabled = true,
                reachable = response.IsSuccessStatusCode,
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
