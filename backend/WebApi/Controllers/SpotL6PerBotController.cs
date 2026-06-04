using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("api/spot-l6-per-bot")]
[Produces("application/json")]
[Authorize]
public class SpotL6PerBotController : ControllerBase
{
    private readonly AppDbContext _context;

    public SpotL6PerBotController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var enabled = await SpotResetConfig.GetPerBotEnabledAsync(_context);
        return Ok(ApiResponse<SpotL6PerBotEnabledResponse>.SuccessResponse(
            new SpotL6PerBotEnabledResponse { Enabled = enabled }));
    }

    [HttpPut]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Set([FromBody] SpotL6PerBotEnabledRequest request)
    {
        var enabled = request.Enabled == true;
        await SpotResetConfig.SavePerBotEnabledAsync(_context, enabled);
        return Ok(ApiResponse<SpotL6PerBotEnabledResponse>.SuccessResponse(
            new SpotL6PerBotEnabledResponse { Enabled = enabled }));
    }
}

public class SpotL6PerBotEnabledRequest
{
    public bool? Enabled { get; set; }
}

public class SpotL6PerBotEnabledResponse
{
    public bool Enabled { get; set; }
}
