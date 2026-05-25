using Contracts.Configuration;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ConfigurationController : ControllerBase
{
    private readonly IConfigurationService _configurationService;
    private readonly AppDbContext _context;

    public ConfigurationController(IConfigurationService configurationService, AppDbContext context)
    {
        _configurationService = configurationService;
        _context = context;
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Configuration>>> GetAll()
    {
        var configurations = await _configurationService.GetAllAsync();
        return Ok(configurations);
    }

    [HttpGet("{key}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Configuration>> GetByKey(string key)
    {
        var configuration = await _configurationService.GetByKeyAsync(key);
        if (configuration == null) return NotFound();
        return Ok(configuration);
    }

    [HttpGet("key/{key}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<Configuration>> GetByKeyAlias(string key) => GetByKey(key);

    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Configuration>> Create(CreateConfigurationRequest request)
    {
        var configuration = await _configurationService.CreateAsync(request);
        return CreatedAtAction(nameof(GetByKey), new { key = configuration.Key }, configuration);
    }

    [HttpPut("{key}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(string key, UpdateConfigurationRequest request)
    {
        var result = await _configurationService.UpdateAsync(key, request);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("{key}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(string key)
    {
        var result = await _configurationService.DeleteAsync(key);
        if (!result) return NotFound();
        return NoContent();
    }
}
