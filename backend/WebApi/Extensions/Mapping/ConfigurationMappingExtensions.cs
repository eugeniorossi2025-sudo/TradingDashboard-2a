using Contracts.Configuration;
using Entities;

namespace WebApi.Extensions.Mapping;

/// <summary>
/// Extension methods for mapping between Configuration entity and DTOs.
/// </summary>
public static class ConfigurationMappingExtensions
{
    /// <summary>
    /// Maps a CreateConfigurationRequest to a Configuration entity.
    /// </summary>
    /// <param name="request">The create configuration request.</param>
    /// <returns>A new Configuration entity.</returns>
    public static Configuration MapToEntity(this CreateConfigurationRequest request)
    {
        return new Configuration
        {
            Key = request.Key,
            Description = request.Description,
            Pos = request.Pos,
            Value = request.Value
        };
    }

    /// <summary>
    /// Converts a CreateConfigurationRequest to a Configuration entity.
    /// </summary>
    /// <param name="request">The create configuration request.</param>
    /// <returns>A new Configuration entity.</returns>
    public static Configuration ToEntity(this CreateConfigurationRequest request)
    {
        return new Configuration
        {
            Key = request.Key,
            Description = request.Description,
            Pos = request.Pos,
            Value = request.Value
        };
    }

    /// <summary>
    /// Updates a Configuration entity from an UpdateConfigurationRequest.
    /// </summary>
    /// <param name="configuration">The configuration entity to update.</param>
    /// <param name="request">The update configuration request.</param>
    public static void UpdateFromRequest(this Configuration configuration, UpdateConfigurationRequest request)
    {
        if (!string.IsNullOrEmpty(request.Description))
            configuration.Description = request.Description;

        if (request.Pos.HasValue)
            configuration.Pos = request.Pos.Value;

        if (!string.IsNullOrEmpty(request.Value))
            configuration.Value = request.Value;
    }

    /// <summary>
    /// Converts a Configuration entity to a contract DTO.
    /// </summary>
    /// <param name="configuration">The configuration entity.</param>
    /// <returns>A configuration contract DTO.</returns>
    public static Configuration ToContract(this Configuration configuration)
    {
        return configuration;
    }

    /// <summary>
    /// Converts a collection of Configuration entities to contract DTOs.
    /// </summary>
    /// <param name="configurations">The configuration entities.</param>
    /// <returns>A collection of configuration contract DTOs.</returns>
    public static IEnumerable<Configuration> ToContracts(this IEnumerable<Configuration> configurations)
    {
        return configurations.Select(c => c.ToContract());
    }
}