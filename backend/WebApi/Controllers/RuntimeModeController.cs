using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("api/runtime-mode")]
[Produces("application/json")]
[Authorize]
public class RuntimeModeController : ControllerBase
{
    private const string RuntimeModeKey = "RUNTIME_MODE";
    private const string Production = "Production";
    private const string Demo = "Demo";

    private readonly AppDbContext _context;

    public RuntimeModeController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<RuntimeModeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRuntimeMode()
    {
        var mode = await GetCurrentModeAsync();
        return Ok(ApiResponse<RuntimeModeResponse>.SuccessResponse(BuildResponse(mode)));
    }

    [HttpPut]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(ApiResponse<RuntimeModeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetRuntimeMode([FromBody] RuntimeModeRequest request)
    {
        var mode = NormalizeMode(request.RuntimeMode ?? (request.IsDemoMode == true ? Demo : Production));

        var setting = await _context.Configurations.FirstOrDefaultAsync(c => c.Key == RuntimeModeKey);
        if (setting == null)
        {
            setting = new Configuration
            {
                Key = RuntimeModeKey,
                Description = "Current accounting runtime mode: Production or Demo",
                Pos = 999,
                Value = mode
            };
            _context.Configurations.Add(setting);
        }
        else
        {
            setting.Value = mode;
        }

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<RuntimeModeResponse>.SuccessResponse(BuildResponse(mode)));
    }

    private async Task<string> GetCurrentModeAsync()
    {
        var value = await _context.Configurations
            .Where(c => c.Key == RuntimeModeKey)
            .Select(c => c.Value)
            .FirstOrDefaultAsync();

        return NormalizeMode(value);
    }

    private static string NormalizeMode(string? value)
    {
        return string.Equals(value, Demo, StringComparison.OrdinalIgnoreCase) ? Demo : Production;
    }

    private static RuntimeModeResponse BuildResponse(string mode)
    {
        return new RuntimeModeResponse
        {
            RuntimeMode = mode,
            IsDemoMode = string.Equals(mode, Demo, StringComparison.OrdinalIgnoreCase)
        };
    }
}

public class RuntimeModeRequest
{
    public string? RuntimeMode { get; set; }
    public bool? IsDemoMode { get; set; }
}

public class RuntimeModeResponse
{
    public string RuntimeMode { get; set; } = "Production";
    public bool IsDemoMode { get; set; }
}
