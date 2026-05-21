using Contracts.User;

namespace WebApi.Extensions.Mapping;

/// <summary>
/// Extension methods for mapping between User entity and DTOs.
/// </summary>
public static class UserMappingExtensions
{
    /// <summary>
    /// Maps a CreateUserRequest to a User entity.
    /// </summary>
    /// <param name="request">The create user request.</param>
    /// <returns>A new User entity.</returns>
    public static Entities.User MapToEntity(this CreateUserRequest request)
    {
        return new Entities.User
        {
            Description = request.Description,
            Admin = request.IsAdmin,
            LastLogin = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates a User entity from an UpdateUserRequest.
    /// </summary>
    /// <param name="user">The user entity to update.</param>
    /// <param name="request">The update user request.</param>
    public static void UpdateFromRequest(this Entities.User user, UpdateUserRequest request)
    {
        if (!string.IsNullOrEmpty(request.Description))
            user.Description = request.Description;

        if (request.IsAdmin.HasValue)
            user.Admin = request.IsAdmin.Value;

        if (request.LastLogin.HasValue)
            user.LastLogin = request.LastLogin.Value;
    }
}