// WebApi/Services/Implementations/UserGridConfigurationService.cs

using Contracts.UserGridConfiguration;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Extensions.Mapping;

namespace WebApi.Services.Implementations;

/// <summary>
/// Service for managing user grid configurations.
/// </summary>
public class UserGridConfigurationService : IUserGridConfigurationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserGridConfigurationService> _logger;

    public UserGridConfigurationService(AppDbContext context, ILogger<UserGridConfigurationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserGridConfigurationResponse> CreateAsync(CreateUserGridConfigurationRequest request)
    {
        var config = request.MapToEntity();

        _context.UserGridConfigurations.Add(config);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Grid configuration created - ID: {Id}, User: {UserId}, Page: {PageName}, Grid: {GridName}, Column: {ColumnName}",
            config.Id, config.IdUser, config.PageName, config.GridName, config.ColumnName);

        return config.ToContract();
    }

    public async Task<UserGridConfigurationResponse?> GetByIdAsync(int id)
    {
        _logger.LogDebug("Getting grid configuration by ID: {Id}", id);

        var config = await _context.UserGridConfigurations.FindAsync(id);

        if (config == null)
        {
            _logger.LogWarning("Grid configuration not found - ID: {Id}", id);
            return null;
        }

        return config.ToContract();
    }

    public async Task<IEnumerable<UserGridConfigurationResponse>> GetByUserPageGridAsync(int userId, string pageName,
        string gridName)
    {
        _logger.LogDebug("Getting grid configurations - User: {UserId}, Page: {PageName}, Grid: {GridName}",
            userId, pageName, gridName);

        var configs = await _context.UserGridConfigurations
            .Where(c => c.IdUser == userId && c.PageName == pageName && c.GridName == gridName)
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} grid configurations", configs.Count);

        return configs.Select(c => c.ToContract());
    }

    public async Task<IEnumerable<UserGridConfigurationResponse>> GetByUserAsync(int userId)
    {
        _logger.LogDebug("Getting all grid configurations for user: {UserId}", userId);

        var configs = await _context.UserGridConfigurations
            .Where(c => c.IdUser == userId)
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} grid configurations for user {UserId}", configs.Count, userId);

        return configs.Select(c => c.ToContract());
    }

    public async Task<bool> UpdateAsync(int id, UpdateUserGridConfigurationRequest request)
    {
        _logger.LogDebug("Updating grid configuration ID: {Id}", id);

        var config = await _context.UserGridConfigurations.FindAsync(id);
        if (config == null)
        {
            _logger.LogWarning("Grid configuration not found for update - ID: {Id}", id);
            return false;
        }

        config.UpdateFromRequest(request);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Grid configuration updated - ID: {Id}", id);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogDebug("Deleting grid configuration ID: {Id}", id);

        var config = await _context.UserGridConfigurations.FindAsync(id);
        if (config == null)
        {
            _logger.LogWarning("Grid configuration not found for deletion - ID: {Id}", id);
            return false;
        }

        _context.UserGridConfigurations.Remove(config);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Grid configuration deleted - ID: {Id}", id);

        return true;
    }

    public async Task ClearAsync(int userId, string pageName, string gridName)
    {
        _logger.LogInformation("Clearing grid configurations - User: {UserId}, Page: {PageName}, Grid: {GridName}",
            userId, pageName, gridName);

        var configs = await _context.UserGridConfigurations
            .Where(c => c.IdUser == userId && c.PageName == pageName && c.GridName == gridName)
            .ToListAsync();

        if (configs.Any())
        {
            _context.UserGridConfigurations.RemoveRange(configs);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cleared {Count} grid configurations", configs.Count);
        }
        else
        {
            _logger.LogDebug("No grid configurations found to clear");
        }
    }
}