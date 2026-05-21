// WebApi/Services/ICommandService.cs

using Contracts.Command;

namespace WebApi.Services;

/// <summary>
/// Interface for command service operations.
/// </summary>
public interface ICommandService
{
    Task<CommandResponse> CreateAsync(CreateCommandRequest request);
    Task<CommandResponse?> GetByIdAsync(int id);
    Task<IEnumerable<CommandResponse>> GetAllAsync(int page = 1, int pageSize = 50);
    Task<IEnumerable<CommandResponse>> GetByPcAsync(string pc, int limit = 50);
    Task<IEnumerable<CommandResponse>> GetPendingByPcAsync(string pc);
    Task<IEnumerable<CommandResponse>> GetByUserAsync(int userId, int limit = 50);
    Task<CommandResponse> StartPcAsync(string pc, int userId);
    Task<CommandResponse> StopPcAsync(string pc, int userId);
    Task<CommandResponse> ResetMartingaleAsync(string pc, int userId);
    Task<bool> DeleteAsync(int id);
}