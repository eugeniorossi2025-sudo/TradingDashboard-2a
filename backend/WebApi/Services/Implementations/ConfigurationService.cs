using Contracts.Configuration;
using Entities;
using WebApi.Data;
using WebApi.Extensions.Mapping;

namespace WebApi.Services.Implementations;

/// <summary>
/// Service for managing configuration operations.
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationService"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public ConfigurationService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all configurations asynchronously.
    /// </summary>
    /// <returns>A collection of all configurations.</returns>
    public Task<IEnumerable<Configuration>> GetAllAsync()
    {
        return Task.FromResult(_context.Configurations.ToContracts());
    }

    /// <summary>
    /// Gets a configuration by its identifier asynchronously.
    /// </summary>
    /// <param name="id">The configuration identifier.</param>
    /// <returns>The configuration if found; otherwise, null.</returns>
    public async Task<Configuration> GetByIdAsync(int id)
    {
        return await _context.Configurations.FindAsync(id) ?? new Configuration();
    }

    /// <summary>
    /// Creates a new configuration asynchronously.
    /// </summary>
    /// <param name="request">The create configuration request.</param>
    /// <returns>The created configuration.</returns>
    public async Task<Configuration> CreateAsync(CreateConfigurationRequest request)
    {
        var configuration = request.MapToEntity();
        _context.Configurations.Add(configuration);
        await _context.SaveChangesAsync();
        return configuration;
    }

    /// <summary>
    /// Updates an existing configuration asynchronously.
    /// </summary>
    /// <param name="id">The configuration identifier.</param>
    /// <param name="request">The update configuration request.</param>
    /// <returns>True if the configuration was updated; otherwise, false.</returns>
    public async Task<bool> UpdateAsync(int id, UpdateConfigurationRequest request)
    {
        var configuration = await _context.Configurations.FindAsync(id);
        if (configuration == null) return false;

        configuration.UpdateFromRequest(request);
        _context.Configurations.Update(configuration);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Deletes a configuration asynchronously.
    /// </summary>
    /// <param name="id">The configuration identifier.</param>
    /// <returns>True if the configuration was deleted; otherwise, false.</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var configuration = await _context.Configurations.FindAsync(id);
        if (configuration == null) return false;

        _context.Configurations.Remove(configuration);
        await _context.SaveChangesAsync();
        return true;
    }
}