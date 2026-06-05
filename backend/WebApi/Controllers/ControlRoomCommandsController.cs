using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Constants;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("api/control-room/commands")]
[Produces("application/json")]
[Authorize]
public class ControlRoomCommandsController : ControllerBase
{
    private readonly IControlRoomCommandOverrideService _overrideService;

    public ControlRoomCommandsController(IControlRoomCommandOverrideService overrideService) =>
        _overrideService = overrideService;

    [HttpPost("continue")]
    [Authorize(Policy = AuthConstants.Policies.RequireAdmin)]
    public async Task<IActionResult> Continue([FromBody] ControlRoomPcCommandRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.Pc))
            return BadRequest(ApiResponse<object>.ErrorResponse("PC obbligatorio"));

        var result = await _overrideService.SetContinueAsync(request.Pc, ResolveUserId(), cancellationToken);
        return Ok(ApiResponse<ControlRoomCommandOverrideResult>.SuccessResponse(
            result,
            $"Comando CONTINUA (AC0) inviato a {result.Pc}"));
    }

    [HttpPost("reset-martingale")]
    [Authorize(Policy = AuthConstants.Policies.RequireAdmin)]
    public async Task<IActionResult> ResetMartingale([FromBody] ControlRoomPcCommandRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.Pc))
            return BadRequest(ApiResponse<object>.ErrorResponse("PC obbligatorio"));

        var result = await _overrideService.SetResetMartingaleAsync(request.Pc, ResolveUserId(), cancellationToken);
        return Ok(ApiResponse<ControlRoomCommandOverrideResult>.SuccessResponse(
            result,
            $"Comando AZZERA MARTINGALA (AC2) inviato a {result.Pc}"));
    }

    private int? ResolveUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(AuthConstants.Claims.UserId);
        return int.TryParse(raw, out var userId) && userId > 0 ? userId : null;
    }
}

public sealed class ControlRoomPcCommandRequest
{
    public string? Pc { get; set; }
}
