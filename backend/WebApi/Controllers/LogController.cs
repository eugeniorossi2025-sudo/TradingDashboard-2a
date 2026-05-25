using Contracts.Log;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LogController : ControllerBase
{
    private readonly ILogService _logService;

    public LogController(ILogService logService)
    {
        _logService = logService;
    }

    /// <summary>
    /// Gets ApiLogs with optional filters for CreatedAt (range), Category, and Action.
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PagedApiLogResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedApiLogResult>> GetAll(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? category,
        [FromQuery] int? action,
        [FromQuery] string? description,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _logService.GetPagedAsync(from, to, category, action, description, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(Log), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Log>> GetById(int id)
    {
        var log = await _logService.GetByIdAsync(id);
        if (log == null) return NotFound();
        return Ok(log);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(Log), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Log>> Create(CreateLogRequest request)
    {
        var log = await _logService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = log.Id }, log);
    }

    [HttpPut("{id:int}")]
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
    /// Deletes ApiLogs matching the given filters.
    /// </summary>
    [HttpDelete]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> DeleteFiltered(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? category,
        [FromQuery] int? action)
    {
        await _logService.DeleteFilteredAsync(from, to, category, action);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
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
