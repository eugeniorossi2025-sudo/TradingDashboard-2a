using Contracts.Log;
using Entities;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Extensions.Mapping;

namespace WebApi.Services.Implementations;

/// <summary>
/// Service for managing log operations.
/// </summary>
public class LogService : ILogService
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogService"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public LogService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all log entries asynchronously.
    /// </summary>
    /// <returns>A collection of all log entries.</returns>
    public async Task<IEnumerable<Log>> GetAllAsync()
    {
        return await _context.Logs.ToListAsync();
    }

    /// <summary>
    /// Gets a log entry by its identifier asynchronously.
    /// </summary>
    /// <param name="id">The log entry identifier.</param>
    /// <returns>The log entry if found; otherwise, null.</returns>
    public async Task<Log> GetByIdAsync(int id)
    {
        return (await _context.Logs.FindAsync(id))!;
    }

    /// <summary>
    /// Creates a new log entry asynchronously.
    /// </summary>
    /// <param name="request">The create log request.</param>
    /// <returns>The created log entry.</returns>
    public async Task<Log> CreateAsync(CreateLogRequest request)
    {
        var log = request.ToEntity();
        _context.Logs.Add(log);
        await _context.SaveChangesAsync();
        return log;
    }

    /// <summary>
    /// Updates an existing log entry asynchronously.
    /// </summary>
    /// <param name="id">The log entry identifier.</param>
    /// <param name="request">The update log request.</param>
    /// <returns>True if the log entry was updated; otherwise, false.</returns>
    public async Task<bool> UpdateAsync(int id, UpdateLogRequest request)
    {
        var log = await _context.Logs.FindAsync(id);
        if (log == null) return false;

        log.UpdateFromRequest(request);
        _context.Logs.Update(log);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Deletes a log entry asynchronously.
    /// </summary>
    /// <param name="id">The log entry identifier.</param>
    /// <returns>True if the log entry was deleted; otherwise, false.</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var log = await _context.Logs.FindAsync(id);
        if (log == null) return false;

        _context.Logs.Remove(log);
        await _context.SaveChangesAsync();
        return true;
    }
}