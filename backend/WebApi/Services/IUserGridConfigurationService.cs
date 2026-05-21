// WebApi/Services/IUserGridConfigurationService.cs

using Contracts.UserGridConfiguration;

namespace WebApi.Services;

public interface IUserGridConfigurationService
{
    Task<UserGridConfigurationResponse> CreateAsync(CreateUserGridConfigurationRequest request);
    Task<UserGridConfigurationResponse?> GetByIdAsync(int id);

    Task<IEnumerable<UserGridConfigurationResponse>> GetByUserPageGridAsync(int userId, string pageName,
        string gridName);

    Task<IEnumerable<UserGridConfigurationResponse>> GetByUserAsync(int userId);
    Task<bool> UpdateAsync(int id, UpdateUserGridConfigurationRequest request);
    Task<bool> DeleteAsync(int id);
    Task ClearAsync(int userId, string pageName, string gridName);
}