using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("api/player-race-8/ac3")]
[Produces("application/json")]
[Authorize]
public class PlayerRace8Ac3Controller : ControllerBase
{
    private readonly AppDbContext _context;
    public PlayerRace8Ac3Controller(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(ApiResponse<PlayerRaceFilterResponse>.SuccessResponse(
            new PlayerRaceFilterResponse { Enabled = await PlayerRaceFilterConfig.GetAsync(_context, PlayerRaceFilterConfig.Race8Ac3Key) }));

    [HttpPut]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Set([FromBody] PlayerRaceFilterRequest request)
    {
        var enabled = request.Enabled == true;
        await PlayerRaceFilterConfig.SaveAsync(_context, PlayerRaceFilterConfig.Race8Ac3Key,
            "Player Race 8 AC3: 1 genera AC3 a 8 PLAYER consecutivi.", 911, enabled);
        var legacy = await _context.Configurations.FirstOrDefaultAsync(c => c.Key == "PLAYER_PACE_FILTER_ENABLED");
        if (legacy != null)
        {
            legacy.Value = enabled ? "1" : "0";
            await _context.SaveChangesAsync();
        }
        return Ok(ApiResponse<PlayerRaceFilterResponse>.SuccessResponse(new PlayerRaceFilterResponse { Enabled = enabled }));
    }
}
