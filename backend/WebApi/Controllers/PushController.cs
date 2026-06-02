using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Constants;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("api/push")]
[Produces("application/json")]
[Authorize]
public class PushController : ControllerBase
{
    private readonly IPushNotificationService _pushNotificationService;

    public PushController(IPushNotificationService pushNotificationService)
    {
        _pushNotificationService = pushNotificationService;
    }

    [HttpGet("vapid-public-key")]
    [AllowAnonymous]
    public IActionResult GetVapidPublicKey()
    {
        var state = _pushNotificationService.GetConfigurationState();
        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            enabled = state.Enabled,
            publicKey = state.PublicKey
        }));
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] PushSubscriptionRequest request, CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(AuthConstants.Claims.UserId)?.Value;
        if (!int.TryParse(userIdValue, out var userId))
            return Unauthorized(ApiResponse<object>.ErrorResponse("User token missing userId claim"));

        try
        {
            await _pushNotificationService.SaveSubscriptionAsync(
                userId,
                request,
                Request.Headers.UserAgent.ToString(),
                cancellationToken);

            return Ok(ApiResponse<object>.SuccessResponse(new object(), "Push subscription saved"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("test")]
    public async Task<IActionResult> SendTest([FromBody] PushTestRequest? request, CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(AuthConstants.Claims.UserId)?.Value;
        if (!int.TryParse(userIdValue, out var userId))
            return Unauthorized(ApiResponse<object>.ErrorResponse("User token missing userId claim"));

        var sent = await _pushNotificationService.SendTestNotificationToUserAsync(
            userId,
            request?.Url,
            cancellationToken);

        var message = sent > 0
            ? "Notifica di prova inviata."
            : "Nessuna subscription attiva trovata per il tuo utente.";

        return Ok(ApiResponse<object>.SuccessResponse(new { sent }, message));
    }
}

public sealed class PushTestRequest
{
    public string? Url { get; set; }
}
