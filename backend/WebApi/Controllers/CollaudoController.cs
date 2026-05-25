using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApi.Models;
using WebApi.Options;
using WebApi.Services;

namespace WebApi.Controllers;

/// <summary>
/// Collaudo mirror: writes PC96 telemetry to dashboard DB (protected by shared secret).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CollaudoController : ControllerBase
{
    public const string MirrorSecretHeader = "X-Collaudo-Mirror-Secret";

    private readonly CollaudoOptions _options;
    private readonly IPcCurrentStatusMirrorService _mirrorService;

    public CollaudoController(
        IOptions<CollaudoOptions> options,
        IPcCurrentStatusMirrorService mirrorService)
    {
        _options = options.Value;
        _mirrorService = mirrorService;
    }

    /// <summary>
    /// Upsert live row on Pc_CurrentStatus (production collaudo, e.g. PC96).
    /// </summary>
    [HttpPost("mirror-pc-status")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> MirrorPcStatus([FromBody] MirrorPcStatusRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_options.MirrorSecret))
            return StatusCode(503, ApiResponse<object>.ErrorResponse("Collaudo mirror secret is not configured on server."));

        if (!ValidateSecret())
            return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid or missing collaudo mirror secret."));

        if (string.IsNullOrWhiteSpace(request.Computer))
            return BadRequest(ApiResponse<object>.ErrorResponse("Computer is required."));

        try
        {
            await _mirrorService.MirrorAsync(request, cancellationToken);
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                request.Computer,
                request.Margine,
                request.Mazzo,
                request.Stato,
                mirroredAt = DateTime.UtcNow
            }, "Pc_CurrentStatus mirrored"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse($"Mirror failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// Read current Pc_CurrentStatus row for collaudo verification (same secret).
    /// </summary>
    [HttpGet("pc-status/{computer}")]
    [ProducesResponseType(typeof(ApiResponse<MirrorPcStatusRequest>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPcStatus(string computer, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_options.MirrorSecret))
            return StatusCode(503, ApiResponse<object>.ErrorResponse("Collaudo mirror secret is not configured on server."));

        if (!ValidateSecret())
            return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid or missing collaudo mirror secret."));

        var row = await _mirrorService.GetPcStatusAsync(computer, cancellationToken);
        if (row == null)
            return NotFound(ApiResponse<object>.ErrorResponse($"PC '{computer}' not found in Pc_CurrentStatus."));

        return Ok(ApiResponse<MirrorPcStatusRequest>.SuccessResponse(row));
    }

    private bool ValidateSecret()
    {
        var configured = _options.MirrorSecret;
        if (string.IsNullOrEmpty(configured))
            return false;

        if (!Request.Headers.TryGetValue(MirrorSecretHeader, out var providedValues))
            return false;

        var provided = providedValues.ToString();
        if (string.IsNullOrEmpty(provided))
            return false;

        var a = Encoding.UTF8.GetBytes(configured);
        var b = Encoding.UTF8.GetBytes(provided);
        return a.Length == b.Length && CryptographicOperations.FixedTimeEquals(a, b);
    }
}
