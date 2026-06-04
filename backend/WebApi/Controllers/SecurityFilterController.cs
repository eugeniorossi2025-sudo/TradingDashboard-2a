using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("api/security-filter")]
[Produces("application/json")]
[Authorize]
public class SecurityFilterController : ControllerBase
{
    private readonly AppDbContext _context;

    public SecurityFilterController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var enabled = await SecurityFilterConfig.GetEnabledAsync(_context);
        var parameters = await SecurityFilterConfig.GetParametersAsync(_context);
        return Ok(ApiResponse<SecurityFilterConfigResponse>.SuccessResponse(new SecurityFilterConfigResponse
        {
            Enabled = enabled,
            MaxAvgSeconds = parameters.MaxAvgSeconds,
            VeryFastSeconds = parameters.VeryFastSeconds,
            MinScore = parameters.MinScore
        }));
    }

    [HttpPut("enabled")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> SetEnabled([FromBody] SecurityFilterEnabledRequest request)
    {
        if (request?.Enabled is not bool enabled)
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Valore non valido: specificare 'enabled' true o false."));

        await SecurityFilterConfig.SaveEnabledAsync(_context, enabled);
        return Ok(ApiResponse<SecurityFilterEnabledResponse>.SuccessResponse(
            new SecurityFilterEnabledResponse { Enabled = enabled }));
    }

    [HttpPut("parameters")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(ApiResponse<SecurityFilterParametersDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetParameters([FromBody] SecurityFilterParametersRequest? request)
    {
        if (request?.MaxAvgSeconds is not decimal maxAvg ||
            request.VeryFastSeconds is not decimal veryFast ||
            request.MinScore is not int minScore)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Parametri non validi: specificare maxAvgSeconds, veryFastSeconds e minScore."));
        }

        if (maxAvg <= 0 || maxAvg > 120)
            return BadRequest(ApiResponse<object>.ErrorResponse(
                $"maxAvgSeconds non valido: {maxAvg}. Valori ammessi: 0.01–120."));

        if (veryFast <= 0 || veryFast > 120)
            return BadRequest(ApiResponse<object>.ErrorResponse(
                $"veryFastSeconds non valido: {veryFast}. Valori ammessi: 0.01–120."));

        if (minScore < 1 || minScore > 4)
            return BadRequest(ApiResponse<object>.ErrorResponse(
                $"minScore non valido: {minScore}. Valori ammessi: 1–4."));

        if (veryFast >= maxAvg)
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "veryFastSeconds deve essere inferiore a maxAvgSeconds."));

        var saved = await SecurityFilterConfig.SaveParametersAsync(_context, maxAvg, veryFast, minScore);
        return Ok(ApiResponse<SecurityFilterParametersDto>.SuccessResponse(saved));
    }
}
