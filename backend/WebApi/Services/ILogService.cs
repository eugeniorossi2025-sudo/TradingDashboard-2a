using Contracts.Log;
using Entities;

namespace WebApi.Services;

/// <summary>
/// Interface for managing log operations.
/// </summary>
public interface ILogService
{
    /// <summary>
    /// Gets all log entries asynchronously.
    /// </summary>
    /// <returns>A collection of all log entries.</returns>
    Task<IEnumerable<Log>> GetAllAsync();

    /// <summary>
    /// Gets a log entry by its identifier asynchronously.
    /// </summary>
    /// <param name="id">The log entry identifier.</param>
    /// <returns>The log entry if found; otherwise, null.</returns>
    Task<Log> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new log entry asynchronously.
    /// </summary>
    /// <param name="request">The create log request.</param>
    /// <returns>The created log entry.</returns>
    Task<Log> CreateAsync(CreateLogRequest request);

    /// <summary>
    /// Updates an existing log entry asynchronously.
    /// </summary>
    /// <param name="id">The log entry identifier.</param>
    /// <param name="request">The update log request.</param>
    /// <returns>True if the log entry was updated; otherwise, false.</returns>
    Task<bool> UpdateAsync(int id, UpdateLogRequest request);

    /// <summary>
    /// Deletes a log entry asynchronously.
    /// </summary>
    /// <param name="id">The log entry identifier.</param>
    /// <returns>True if the log entry was deleted; otherwise, false.</returns>
    Task<bool> DeleteAsync(int id);
}