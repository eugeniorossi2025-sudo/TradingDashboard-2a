// WebApi/Extensions/Mapping/UserGridConfigurationMappingExtensions.cs

using Contracts.UserGridConfiguration;
using Entities;

namespace WebApi.Extensions.Mapping;

/// <summary>
/// Extension methods for mapping UserGridConfiguration entities.
/// </summary>
public static class UserGridConfigurationMappingExtensions
{
    /// <summary>
    /// Maps a CreateUserGridConfigurationRequest to a UserGridConfiguration entity.
    /// </summary>
    public static UserGridConfiguration MapToEntity(this CreateUserGridConfigurationRequest request)
    {
        return new UserGridConfiguration
        {
            IdUser = request.IdUser,
            PageName = request.PageName,
            GridName = request.GridName,
            ColumnName = request.ColumnName,
            Display = request.Display
        };
    }

    /// <summary>
    /// Updates an existing UserGridConfiguration entity from UpdateUserGridConfigurationRequest.
    /// </summary>
    public static void UpdateFromRequest(this UserGridConfiguration config, UpdateUserGridConfigurationRequest request)
    {
        if (request.PageName != null) config.PageName = request.PageName;
        if (request.GridName != null) config.GridName = request.GridName;
        if (request.ColumnName != null) config.ColumnName = request.ColumnName;
        config.Display = request.Display;
    }

    /// <summary>
    /// Maps a UserGridConfiguration entity to UserGridConfigurationResponse.
    /// </summary>
    public static UserGridConfigurationResponse ToContract(this UserGridConfiguration config)
    {
        return new UserGridConfigurationResponse
        {
            Id = config.Id,
            IdUser = config.IdUser,
            PageName = config.PageName!,
            GridName = config.GridName!,
            ColumnName = config.ColumnName!,
            Display = config.Display
        };
    }
}