using Contracts.Configuration;
using Entities;

namespace WebApi.Services;

/// <summary>
/// Interface for managing configuration operations.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Gets all configurations asynchronously.
    /// </summary>
    /// <returns>A collection of all configurations.</returns>
    Task<IEnumerable<Configuration>> GetAllAsync();

    /// <summary>
    /// Gets a configuration by its identifier asynchronously.
    /// </summary>
    /// <param name="id">The configuration identifier.</param>
    /// <returns>The configuration if found; otherwise, null.</returns>
    Task<Configuration> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new configuration asynchronously.
    /// </summary>
    /// <param name="request">The create configuration request.</param>
    /// <returns>The created configuration.</returns>
    Task<Configuration> CreateAsync(CreateConfigurationRequest request);

    /// <summary>
    /// Updates an existing configuration asynchronously.
    /// </summary>
    /// <param name="id">The configuration identifier.</param>
    /// <param name="request">The update configuration request.</param>
    /// <returns>True if the configuration was updated; otherwise, false.</returns>
    Task<bool> UpdateAsync(int id, UpdateConfigurationRequest request);

    /// <summary>
    /// Deletes a configuration asynchronously.
    /// </summary>
    /// <param name="id">The configuration identifier.</param>
    /// <returns>True if the configuration was deleted; otherwise, false.</returns>
    Task<bool> DeleteAsync(int id);
}