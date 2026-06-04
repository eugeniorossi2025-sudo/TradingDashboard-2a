using System.Security.Claims;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApi.Constants;
using WebApi.Data;
using WebApi.Models;
using WebApi.Options;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("api/root-owner")]
[Authorize]
[Produces("application/json")]
public class RootOwnerController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IRootOwnerGuard _guard;
    private readonly IRootOwnerAuditService _audit;
    private readonly IRootOwnerSchemaService _schema;
    private readonly IMissionLifecycleService _missionLifecycle;
    private readonly DeciderOptions _deciderOptions;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RootOwnerController> _logger;

    public RootOwnerController(
        AppDbContext context,
        IRootOwnerGuard guard,
        IRootOwnerAuditService audit,
        IRootOwnerSchemaService schema,
        IMissionLifecycleService missionLifecycle,
        IOptions<DeciderOptions> deciderOptions,
        IHttpClientFactory httpClientFactory,
        ILogger<RootOwnerController> logger)
    {
        _context = context;
        _guard = guard;
        _audit = audit;
        _schema = schema;
        _missionLifecycle = missionLifecycle;
        _deciderOptions = deciderOptions.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<RootOwnerMeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        await _schema.EnsureSchemaAsync(cancellationToken);
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var row = await _context.Users.AsNoTracking()
            .Where(u => u.Id == userId.Value)
            .Select(u => new { u.IsRootOwner, u.UserName })
            .FirstOrDefaultAsync(cancellationToken);

        if (row == null)
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

        return Ok(ApiResponse<RootOwnerMeResponse>.SuccessResponse(new RootOwnerMeResponse
        {
            UserId = userId.Value,
            Username = row.UserName ?? string.Empty,
            IsRootOwner = row.IsRootOwner
        }));
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(ApiResponse<RootOwnerStatusResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
    {
        var deny = await DenyUnlessRootOwnerAsync(cancellationToken);
        if (deny != null) return deny;

        await _schema.EnsureSchemaAsync(cancellationToken);
        var systemState = await GetSystemStateAsync(cancellationToken);
        var mission = await _missionLifecycle.GetCurrentAsync(cancellationToken);
        var botRows = await _context.PcCurrentStatuses.AsNoTracking().ToListAsync(cancellationToken);
        var activeBots = botRows.Count(row =>
            !string.IsNullOrWhiteSpace(row.Stato) &&
            !row.Stato.Contains("off", StringComparison.OrdinalIgnoreCase) &&
            !row.Stato.Contains("stop", StringComparison.OrdinalIgnoreCase));

        var dbOk = await CanQueryDatabaseAsync(cancellationToken);
        var apiProbe = await ProbeDeciderAsync(cancellationToken);
        var audits = await _audit.GetRecentAsync(15, cancellationToken);

        return Ok(ApiResponse<RootOwnerStatusResponse>.SuccessResponse(new RootOwnerStatusResponse
        {
            SystemState = systemState,
            Api = apiProbe,
            Database = new RootOwnerDatabaseStatus { Ok = dbOk },
            ActiveBots = activeBots,
            TotalBotRows = botRows.Count,
            ActiveMission = mission.HasOpenMission
                ? new RootOwnerMissionSummary
                {
                    SessionId = mission.SessionId,
                    RuntimeMode = mission.RuntimeMode,
                    StartTimeUtc = mission.StartTime,
                    TotalMargin = mission.TotalMargin,
                    Completed = mission.Completed
                }
                : null,
            RecentAudits = audits.ToList()
        }));
    }

    [HttpPost("commands/pause-system")]
    public Task<IActionResult> PauseSystem([FromBody] RootOwnerCommandRequest? request, CancellationToken cancellationToken)
        => RunCommandAsync("PAUSE_SYSTEM", RootOwnerConstants.StatePaused, request?.Reason, cancellationToken);

    [HttpPost("commands/blackout-system")]
    public Task<IActionResult> BlackoutSystem([FromBody] RootOwnerCommandRequest? request, CancellationToken cancellationToken)
        => RunCommandAsync("BLACKOUT_SYSTEM", RootOwnerConstants.StateBlackout, request?.Reason, cancellationToken);

    [HttpPost("commands/reactivate-system")]
    public Task<IActionResult> ReactivateSystem([FromBody] RootOwnerCommandRequest? request, CancellationToken cancellationToken)
        => RunCommandAsync("REACTIVATE_SYSTEM", RootOwnerConstants.StateNormal, request?.Reason, cancellationToken);

    [HttpPost("commands/stop-all-bots")]
    public async Task<IActionResult> StopAllBots([FromBody] RootOwnerCommandRequest? request, CancellationToken cancellationToken)
    {
        var deny = await DenyUnlessRootOwnerAsync(cancellationToken);
        if (deny != null) return deny;

        var (actorId, actorName) = GetActor();
        var reason = request?.Reason ?? "Root owner emergency stop";

        try
        {
            if (!_deciderOptions.Enabled)
            {
                await _audit.WriteAsync(actorId, actorName, "STOP_ALL_BOTS", "FAILED", reason, "{\"error\":\"decider_disabled\"}", HttpContext, cancellationToken);
                return StatusCode(502, ApiResponse<object>.ErrorResponse("Decider integration disabled."));
            }

            var url = _deciderOptions.ProactiveUrl("emergency-stop");
            var client = _httpClientFactory.CreateClient(nameof(DeciderController));
            client.Timeout = TimeSpan.FromSeconds(15);
            var response = await client.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                await _audit.WriteAsync(actorId, actorName, "STOP_ALL_BOTS", "FAILED", reason, $"{{\"url\":\"{url}\",\"status\":{(int)response.StatusCode}}}", HttpContext, cancellationToken);
                return StatusCode(502, ApiResponse<object>.ErrorResponse($"Decider responded {(int)response.StatusCode}"));
            }

            await _audit.WriteAsync(actorId, actorName, "STOP_ALL_BOTS", "OK", reason, $"{{\"url\":\"{url}\"}}", HttpContext, cancellationToken);
            return Ok(ApiResponse<object>.SuccessResponse(new { url }, "Stop tutti i bot inviato"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Root owner STOP_ALL_BOTS failed");
            await _audit.WriteAsync(actorId, actorName, "STOP_ALL_BOTS", "FAILED", reason, $"{{\"error\":\"{ex.Message}\"}}", HttpContext, cancellationToken);
            return StatusCode(502, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("commands/stop-active-mission")]
    public async Task<IActionResult> StopActiveMission([FromBody] RootOwnerCommandRequest? request, CancellationToken cancellationToken)
    {
        var deny = await DenyUnlessRootOwnerAsync(cancellationToken);
        if (deny != null) return deny;

        var (actorId, actorName) = GetActor();
        var reason = request?.Reason ?? "RootOwnerStop";

        try
        {
            var result = await _missionLifecycle.FinalizeCurrentAsync(reason, cancellationToken);
            var outcome = result.MissionFinalized ? "OK" : "NO_OPEN_MISSION";
            await _audit.WriteAsync(
                actorId,
                actorName,
                "STOP_ACTIVE_MISSION",
                outcome,
                reason,
                System.Text.Json.JsonSerializer.Serialize(new
                {
                    result.MissionSessionId,
                    result.MissionFinalized,
                    result.Message
                }),
                HttpContext,
                cancellationToken);

            return Ok(ApiResponse<object>.SuccessResponse(result, result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Root owner STOP_ACTIVE_MISSION failed");
            await _audit.WriteAsync(actorId, actorName, "STOP_ACTIVE_MISSION", "FAILED", reason, $"{{\"error\":\"{ex.Message}\"}}", HttpContext, cancellationToken);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("audit/recent")]
    public async Task<IActionResult> GetRecentAudit([FromQuery] int limit = 20, CancellationToken cancellationToken = default)
    {
        var deny = await DenyUnlessRootOwnerAsync(cancellationToken);
        if (deny != null) return deny;

        var rows = await _audit.GetRecentAsync(limit, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<RootOwnerAuditDto>>.SuccessResponse(rows));
    }

    private async Task<IActionResult> RunCommandAsync(
        string action,
        string newState,
        string? reason,
        CancellationToken cancellationToken)
    {
        var deny = await DenyUnlessRootOwnerAsync(cancellationToken);
        if (deny != null) return deny;

        var (actorId, actorName) = GetActor();
        reason ??= action;

        try
        {
            await SetSystemStateAsync(newState, cancellationToken);
            await _audit.WriteAsync(
                actorId,
                actorName,
                action,
                "OK",
                reason,
                $"{{\"systemState\":\"{newState}\"}}",
                HttpContext,
                cancellationToken);

            return Ok(ApiResponse<object>.SuccessResponse(new { systemState = newState }, $"{action} eseguito"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Root owner command {Action} failed", action);
            await _audit.WriteAsync(actorId, actorName, action, "FAILED", reason, $"{{\"error\":\"{ex.Message}\"}}", HttpContext, cancellationToken);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    private async Task<string> GetSystemStateAsync(CancellationToken cancellationToken)
    {
        var value = await _context.Configurations.AsNoTracking()
            .Where(c => c.Key == RootOwnerConstants.SystemStateKey)
            .Select(c => c.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return NormalizeState(value);
    }

    private async Task SetSystemStateAsync(string state, CancellationToken cancellationToken)
    {
        var setting = await _context.Configurations.FirstOrDefaultAsync(c => c.Key == RootOwnerConstants.SystemStateKey, cancellationToken);
        if (setting == null)
        {
            setting = new Configuration
            {
                Key = RootOwnerConstants.SystemStateKey,
                Description = "Root owner global system state: Normal, Paused, Blackout",
                Pos = 998,
                Value = state
            };
            _context.Configurations.Add(setting);
        }
        else
        {
            setting.Value = state;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeState(string? value)
    {
        if (string.Equals(value, RootOwnerConstants.StatePaused, StringComparison.OrdinalIgnoreCase))
            return RootOwnerConstants.StatePaused;
        if (string.Equals(value, RootOwnerConstants.StateBlackout, StringComparison.OrdinalIgnoreCase))
            return RootOwnerConstants.StateBlackout;
        return RootOwnerConstants.StateNormal;
    }

    private async Task<bool> CanQueryDatabaseAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _context.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<RootOwnerApiStatus> ProbeDeciderAsync(CancellationToken cancellationToken)
    {
        if (!_deciderOptions.Enabled)
        {
            return new RootOwnerApiStatus
            {
                Enabled = false,
                Reachable = false,
                Url = _deciderOptions.ApiBaseUrl,
                Message = "Decider disabled in configuration"
            };
        }

        var client = _httpClientFactory.CreateClient(nameof(DeciderController));
        client.Timeout = TimeSpan.FromSeconds(8);
        try
        {
            var response = await client.GetAsync(_deciderOptions.ApiBaseUrl, cancellationToken);
            return new RootOwnerApiStatus
            {
                Enabled = true,
                Reachable = true,
                StatusCode = (int)response.StatusCode,
                Url = _deciderOptions.ApiBaseUrl
            };
        }
        catch (Exception ex)
        {
            return new RootOwnerApiStatus
            {
                Enabled = true,
                Reachable = false,
                Url = _deciderOptions.ApiBaseUrl,
                Message = ex.Message
            };
        }
    }

    private (int? actorId, string? actorName) GetActor()
    {
        var userId = GetCurrentUserId();
        var name = User.FindFirst(ClaimTypes.Name)?.Value
            ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.UniqueName)?.Value
            ?? User.Identity?.Name;
        return (userId, name);
    }

    private int? GetCurrentUserId()
    {
        var userIdValue = User.FindFirst(AuthConstants.Claims.UserId)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdValue, out var userId) ? userId : null;
    }

    private async Task<IActionResult?> DenyUnlessRootOwnerAsync(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Authentication required."));

        if (User.HasClaim(AuthConstants.Claims.IsRootOwner, "true"))
            return null;

        if (await _guard.IsRootOwnerAsync(userId.Value, cancellationToken))
            return null;

        return _guard.ForbiddenRootOwnerOnly();
    }
}

public class RootOwnerMeResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool IsRootOwner { get; set; }
}

public class RootOwnerCommandRequest
{
    public string? Reason { get; set; }
}

public class RootOwnerStatusResponse
{
    public string SystemState { get; set; } = RootOwnerConstants.StateNormal;
    public RootOwnerApiStatus Api { get; set; } = new();
    public RootOwnerDatabaseStatus Database { get; set; } = new();
    public int ActiveBots { get; set; }
    public int TotalBotRows { get; set; }
    public RootOwnerMissionSummary? ActiveMission { get; set; }
    public List<RootOwnerAuditDto> RecentAudits { get; set; } = new();
}

public class RootOwnerApiStatus
{
    public bool Enabled { get; set; }
    public bool Reachable { get; set; }
    public int StatusCode { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Message { get; set; }
}

public class RootOwnerDatabaseStatus
{
    public bool Ok { get; set; }
}

public class RootOwnerMissionSummary
{
    public int? SessionId { get; set; }
    public string RuntimeMode { get; set; } = "Production";
    public DateTime? StartTimeUtc { get; set; }
    public decimal TotalMargin { get; set; }
    public bool Completed { get; set; }
}
