using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("api/player-race-8/filter")]
[Produces("application/json")]
[Authorize]
public class PlayerRace8FilterController : ControllerBase
{
    private readonly AppDbContext _context;
    public PlayerRace8FilterController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(ApiResponse<PlayerRaceFilterResponse>.SuccessResponse(
            new PlayerRaceFilterResponse { Enabled = await PlayerRaceFilterConfig.GetAsync(_context, PlayerRaceFilterConfig.Race8FilterKey) }));

    [HttpPut]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Set([FromBody] PlayerRaceFilterRequest request)
    {
        var enabled = request.Enabled == true;
        await PlayerRaceFilterConfig.SaveAsync(_context, PlayerRaceFilterConfig.Race8FilterKey,
            "Player Race 8 filtro: 1 mostra avviso a 8 PLAYER consecutivi.", 910, enabled);
        return Ok(ApiResponse<PlayerRaceFilterResponse>.SuccessResponse(new PlayerRaceFilterResponse { Enabled = enabled }));
    }
}
