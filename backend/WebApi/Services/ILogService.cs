using Contracts.Log;
using Entities;

namespace WebApi.Services;

public interface ILogService
{
    Task<PagedApiLogResult> GetPagedAsync(
        DateTime? from,
        DateTime? to,
        string? category,
        int? action,
        string? description,
        int page,
        int pageSize);

    Task<IEnumerable<Log>> GetAllAsync();

    Task<Log?> GetByIdAsync(int id);

    Task<Log> CreateAsync(CreateLogRequest request);

    Task<bool> UpdateAsync(int id, UpdateLogRequest request);

    Task<bool> DeleteAsync(int id);

    Task<int> DeleteFilteredAsync(DateTime? from, DateTime? to, string? category, int? action);
}
