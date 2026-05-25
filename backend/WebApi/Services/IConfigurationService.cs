using Contracts.Configuration;
using Entities;

namespace WebApi.Services;

public interface IConfigurationService
{
    Task<IEnumerable<Configuration>> GetAllAsync();

    Task<Configuration?> GetByKeyAsync(string key);

    Task<Configuration> CreateAsync(CreateConfigurationRequest request);

    Task<bool> UpdateAsync(string key, UpdateConfigurationRequest request);

    Task<bool> DeleteAsync(string key);
}
