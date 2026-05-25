using Contracts.Configuration;
using Entities;
using WebApi.Data;
using WebApi.Extensions.Mapping;

namespace WebApi.Services.Implementations;

public class ConfigurationService : IConfigurationService
{
    private readonly AppDbContext _context;

    public ConfigurationService(AppDbContext context)
    {
        _context = context;
    }

    public Task<IEnumerable<Configuration>> GetAllAsync()
    {
        return Task.FromResult(_context.Configurations.ToContracts());
    }

    public async Task<Configuration?> GetByKeyAsync(string key)
    {
        return await _context.Configurations.FindAsync(key);
    }

    public async Task<Configuration> CreateAsync(CreateConfigurationRequest request)
    {
        var configuration = request.MapToEntity();
        _context.Configurations.Add(configuration);
        await _context.SaveChangesAsync();
        return configuration;
    }

    public async Task<bool> UpdateAsync(string key, UpdateConfigurationRequest request)
    {
        var configuration = await _context.Configurations.FindAsync(key);
        if (configuration == null) return false;

        configuration.UpdateFromRequest(request);
        _context.Configurations.Update(configuration);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(string key)
    {
        var configuration = await _context.Configurations.FindAsync(key);
        if (configuration == null) return false;

        _context.Configurations.Remove(configuration);
        await _context.SaveChangesAsync();
        return true;
    }
}
