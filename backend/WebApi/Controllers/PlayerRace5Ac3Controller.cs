using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("api/player-race-5/ac3")]
[Produces("application/json")]
[Authorize]
public class PlayerRace5Ac3Controller : ControllerBase
{
    private readonly AppDbContext _context;
    public PlayerRace5Ac3Controller(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(ApiResponse<PlayerRaceFilterResponse>.SuccessResponse(
            new PlayerRaceFilterResponse { Enabled = await PlayerRaceFilterConfig.GetAsync(_context, PlayerRaceFilterConfig.Race5Ac3Key) }));

    [HttpPut]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Set([FromBody] PlayerRaceFilterRequest request)
    {
        var enabled = request.Enabled == true;
        await PlayerRaceFilterConfig.SaveAsync(_context, PlayerRaceFilterConfig.Race5Ac3Key,
            "Player Race 5 AC3: 1 genera AC3 a 5 PLAYER consecutivi.", 909, enabled);
        return Ok(ApiResponse<PlayerRaceFilterResponse>.SuccessResponse(new PlayerRaceFilterResponse { Enabled = enabled }));
    }
}
