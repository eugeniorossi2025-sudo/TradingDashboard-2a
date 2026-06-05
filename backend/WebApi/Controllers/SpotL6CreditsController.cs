using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("api/spot-l6-credits")]
[Produces("application/json")]
[Authorize]
public class SpotL6CreditsController : ControllerBase
{
    private readonly AppDbContext _context;

    public SpotL6CreditsController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var (creditL5Required, creditsGenerated) = await SpotResetConfig.GetCreditsAsync(_context);
        return Ok(ApiResponse<SpotL6CreditsResponse>.SuccessResponse(
            new SpotL6CreditsResponse
            {
                CreditL5Required = creditL5Required,
                CreditsGenerated = creditsGenerated
            }));
    }

    [HttpPut]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(ApiResponse<SpotL6CreditsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Set([FromBody] SpotL6CreditsRequest? request)
    {
        if (request?.CreditL5Required is not int creditL5Required)
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Config crediti L6 non valida: specificare 'creditL5Required' intero >= 1."));

        if (request.CreditsGenerated is not int creditsGenerated)
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Config crediti L6 non valida: specificare 'creditsGenerated' intero >= 1."));

        if (creditL5Required < 1 || creditL5Required > 99)
            return BadRequest(ApiResponse<object>.ErrorResponse(
                $"creditL5Required non valido: {creditL5Required}. Valori ammessi: 1-99."));

        if (creditsGenerated < 1 || creditsGenerated > 99)
            return BadRequest(ApiResponse<object>.ErrorResponse(
                $"creditsGenerated non valido: {creditsGenerated}. Valori ammessi: 1-99."));

        await SpotResetConfig.SaveCreditsAsync(_context, creditL5Required, creditsGenerated);
        return Ok(ApiResponse<SpotL6CreditsResponse>.SuccessResponse(
            new SpotL6CreditsResponse
            {
                CreditL5Required = creditL5Required,
                CreditsGenerated = creditsGenerated
            }));
    }
}

public class SpotL6CreditsRequest
{
    public int? CreditL5Required { get; set; }
    public int? CreditsGenerated { get; set; }
}

public class SpotL6CreditsResponse
{
    public int CreditL5Required { get; set; }
    public int CreditsGenerated { get; set; }
}
