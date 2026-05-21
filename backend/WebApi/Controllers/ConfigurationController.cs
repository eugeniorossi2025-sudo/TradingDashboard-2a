using Contracts.Configuration;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Services;

namespace WebApi.Controllers;

/// <summary>
/// Controller for managing configuration settings.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ConfigurationController : ControllerBase
{
    private readonly IConfigurationService _configurationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationController"/> class.
    /// </summary>
    /// <param name="configurationService">The configuration service.</param>
    public ConfigurationController(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }

    /// <summary>
    /// Gets all configurations.
    /// </summary>
    /// <returns>A collection of all configurations.</returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Configuration>>> GetAll()
    {
        var configurations = await _configurationService.GetAllAsync();
        return Ok(configurations);
    }

    /// <summary>
    /// Gets a configuration by its identifier.
    /// </summary>
    /// <param name="id">The configuration identifier.</param>
    /// <returns>The configuration if found; otherwise, a not found result.</returns>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Configuration>> GetById(int id)
    {
        var configuration = await _configurationService.GetByIdAsync(id);
        if (configuration == null) return NotFound();
        return Ok(configuration);
    }

    /// <summary>
    /// Creates a new configuration.
    /// </summary>
    /// <param name="request">The create configuration request.</param>
    /// <returns>The created configuration.</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Configuration>> Create(CreateConfigurationRequest request)
    {
        var configuration = await _configurationService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = configuration.Id }, configuration);
    }

    /// <summary>
    /// Updates an existing configuration.
    /// </summary>
    /// <param name="id">The configuration identifier.</param>
    /// <param name="request">The update configuration request.</param>
    /// <returns>A no content result if successful; otherwise, a not found result.</returns>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(int id, UpdateConfigurationRequest request)
    {
        var result = await _configurationService.UpdateAsync(id, request);
        if (!result) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Deletes a configuration.
    /// </summary>
    /// <param name="id">The configuration identifier.</param>
    /// <returns>A no content result if successful; otherwise, a not found result.</returns>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _configurationService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}