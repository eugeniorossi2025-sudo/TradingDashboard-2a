using Contracts.Log;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Services;

namespace WebApi.Controllers;

/// <summary>
/// Controller for managing log entries.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LogController : ControllerBase
{
    private readonly ILogService _logService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogController"/> class.
    /// </summary>
    /// <param name="logService">The log service.</param>
    public LogController(ILogService logService)
    {
        _logService = logService;
    }

    /// <summary>
    /// Gets all log entries.
    /// </summary>
    /// <returns>A collection of all log entries.</returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Log>>> GetAll()
    {
        var logs = await _logService.GetAllAsync();
        return Ok(logs);
    }

    /// <summary>
    /// Gets a log entry by its identifier.
    /// </summary>
    /// <param name="id">The log entry identifier.</param>
    /// <returns>The log entry if found; otherwise, a not found result.</returns>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Log>> GetById(int id)
    {
        var log = await _logService.GetByIdAsync(id);
        if (log == null) return NotFound();
        return Ok(log);
    }

    /// <summary>
    /// Creates a new log entry.
    /// </summary>
    /// <param name="request">The create log request.</param>
    /// <returns>The created log entry.</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Log>> Create(CreateLogRequest request)
    {
        var log = await _logService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = log.Id }, log);
    }

    /// <summary>
    /// Updates an existing log entry.
    /// </summary>
    /// <param name="id">The log entry identifier.</param>
    /// <param name="request">The update log request.</param>
    /// <returns>A no content result if successful; otherwise, a not found result.</returns>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(int id, UpdateLogRequest request)
    {
        var result = await _logService.UpdateAsync(id, request);
        if (!result) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Deletes a log entry.
    /// </summary>
    /// <param name="id">The log entry identifier.</param>
    /// <returns>A no content result if successful; otherwise, a not found result.</returns>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _logService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}