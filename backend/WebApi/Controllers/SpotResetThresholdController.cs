using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("api/spot-reset/threshold")]
[Produces("application/json")]
[Authorize]
public class SpotResetThresholdController : ControllerBase
{
    private readonly AppDbContext _context;

    public SpotResetThresholdController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var threshold = await SpotResetConfig.GetThresholdAsync(_context);
        return Ok(ApiResponse<SpotResetThresholdResponse>.SuccessResponse(
            new SpotResetThresholdResponse { Threshold = threshold }));
    }

    [HttpPut]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(ApiResponse<SpotResetThresholdResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Set([FromBody] SpotResetThresholdRequest? request)
    {
        if (request?.Threshold is not int threshold)
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Soglia L6 non valida: specificare 'threshold' intero tra 1 e 99."));

        if (threshold < 1 || threshold > 99)
            return BadRequest(ApiResponse<object>.ErrorResponse(
                $"Soglia L6 non valida: {threshold}. Valori ammessi: 1–99."));

        await SpotResetConfig.SaveThresholdAsync(_context, threshold);
        return Ok(ApiResponse<SpotResetThresholdResponse>.SuccessResponse(
            new SpotResetThresholdResponse { Threshold = threshold }));
    }
}
