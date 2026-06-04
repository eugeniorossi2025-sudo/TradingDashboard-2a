using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("api/player-pace-filter")]
[Produces("application/json")]
[Authorize]
public class PlayerPaceFilterController : ControllerBase
{
    public const string PlayerPaceFilterKey = "PLAYER_PACE_FILTER_ENABLED";

    private readonly AppDbContext _context;

    public PlayerPaceFilterController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PlayerPaceFilterResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlayerPaceFilter()
    {
        var enabled = await GetCurrentEnabledAsync();
        return Ok(ApiResponse<PlayerPaceFilterResponse>.SuccessResponse(BuildResponse(enabled)));
    }

    [HttpPut]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(ApiResponse<PlayerPaceFilterResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetPlayerPaceFilter([FromBody] PlayerPaceFilterRequest request)
    {
        var enabled = request.Enabled == true;
        var setting = await _context.Configurations.FirstOrDefaultAsync(c => c.Key == PlayerPaceFilterKey);
        if (setting == null)
        {
            setting = new Configuration
            {
                Key = PlayerPaceFilterKey,
                Description = "Player Pace filter operativo: 1 attivo (anomalia P1-P5 genera AC3), 0 spento.",
                Pos = 907,
                Value = enabled ? "1" : "0"
            };
            _context.Configurations.Add(setting);
        }
        else
        {
            setting.Value = enabled ? "1" : "0";
        }

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<PlayerPaceFilterResponse>.SuccessResponse(BuildResponse(enabled)));
    }

    internal static bool ParseEnabledFlag(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return true;

        return value.Trim().Equals("1", StringComparison.OrdinalIgnoreCase)
            || value.Trim().Equals("true", StringComparison.OrdinalIgnoreCase)
            || value.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase)
            || value.Trim().Equals("on", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<bool> GetCurrentEnabledAsync()
    {
        var value = await _context.Configurations
            .AsNoTracking()
            .Where(c => c.Key == PlayerPaceFilterKey)
            .Select(c => c.Value)
            .FirstOrDefaultAsync();

        return ParseEnabledFlag(value);
    }

    private static PlayerPaceFilterResponse BuildResponse(bool enabled)
    {
        return new PlayerPaceFilterResponse { Enabled = enabled };
    }
}

public class PlayerPaceFilterRequest
{
    public bool? Enabled { get; set; }
}

public class PlayerPaceFilterResponse
{
    public bool Enabled { get; set; }
}
