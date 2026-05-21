// WebApi/Services/IValueService.cs

using Contracts.Value;

namespace WebApi.Services;

/// <summary>
/// Interface for value (telemetry) service operations.
/// </summary>
public interface IValueService
{
    /// <summary>
    /// Creates a new telemetry value.
    /// </summary>
    Task<ValueResponse> CreateAsync(CreateValueRequest request);

    /// <summary>
    /// Gets a value by ID.
    /// </summary>
    Task<ValueResponse?> GetByIdAsync(int id);

    /// <summary>
    /// Gets all values with pagination.
    /// </summary>
    Task<IEnumerable<ValueResponse>> GetAllAsync(int page = 1, int pageSize = 50);

    /// <summary>
    /// Gets values by account name.
    /// </summary>
    Task<IEnumerable<ValueResponse>> GetByAccountAsync(string account, int limit = 100);

    /// <summary>
    /// Gets latest values (one per account/table).
    /// </summary>
    Task<IEnumerable<ValueResponse>> GetLatestAsync();

    /// <summary>
    /// Gets values by date range.
    /// </summary>
    Task<IEnumerable<ValueResponse>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Updates an existing value.
    /// </summary>
    Task<bool> UpdateAsync(int id, UpdateValueRequest request);

    /// <summary>
    /// Deletes a value.
    /// </summary>
    Task<bool> DeleteAsync(int id);
}