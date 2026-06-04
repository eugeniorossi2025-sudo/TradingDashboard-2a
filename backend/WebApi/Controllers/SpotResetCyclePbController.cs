using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("api/spot-reset/cycle-pb-hands")]
[Produces("application/json")]
[Authorize]
public class SpotResetCyclePbController : ControllerBase
{
    private readonly AppDbContext _context;

    public SpotResetCyclePbController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var hands = await SpotResetConfig.GetCyclePbHandsAsync(_context);
        return Ok(ApiResponse<SpotResetCyclePbHandsResponse>.SuccessResponse(
            new SpotResetCyclePbHandsResponse { Hands = hands }));
    }

    [HttpPut]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(ApiResponse<SpotResetCyclePbHandsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Set([FromBody] SpotResetCyclePbHandsRequest? request)
    {
        if (request?.Hands is not int hands)
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Ciclo PB non valido: specificare 'hands' intero tra 1 e 99999."));

        if (hands < 1 || hands > 99999)
            return BadRequest(ApiResponse<object>.ErrorResponse(
                $"Ciclo PB non valido: {hands}. Valori ammessi: 1–99999."));

        await SpotResetConfig.SaveCyclePbHandsAsync(_context, hands);
        return Ok(ApiResponse<SpotResetCyclePbHandsResponse>.SuccessResponse(
            new SpotResetCyclePbHandsResponse { Hands = hands }));
    }
}

public class SpotResetCyclePbHandsRequest
{
    public int? Hands { get; set; }
}

public class SpotResetCyclePbHandsResponse
{
    public int Hands { get; set; }
}
