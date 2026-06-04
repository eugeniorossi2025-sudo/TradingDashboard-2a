using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("api/player-race-5/filter")]
[Produces("application/json")]
[Authorize]
public class PlayerRace5FilterController : ControllerBase
{
    private readonly AppDbContext _context;
    public PlayerRace5FilterController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(ApiResponse<PlayerRaceFilterResponse>.SuccessResponse(
            new PlayerRaceFilterResponse { Enabled = await PlayerRaceFilterConfig.GetAsync(_context, PlayerRaceFilterConfig.Race5FilterKey) }));

    [HttpPut]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Set([FromBody] PlayerRaceFilterRequest request)
    {
        var enabled = request.Enabled == true;
        await PlayerRaceFilterConfig.SaveAsync(_context, PlayerRaceFilterConfig.Race5FilterKey,
            "Player Race 5 filtro: 1 mostra avviso a 5 PLAYER consecutivi.", 908, enabled);
        return Ok(ApiResponse<PlayerRaceFilterResponse>.SuccessResponse(new PlayerRaceFilterResponse { Enabled = enabled }));
    }
}
