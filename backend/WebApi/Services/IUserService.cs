using Contracts.User;
using Entities;

namespace WebApi.Services;

/// <summary>
/// Interface for managing user operations and authentication.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets all users asynchronously.
    /// </summary>
    /// <returns>A collection of all <see cref="User"/> entities.</returns>
    Task<IEnumerable<User>> GetAllAsync();

    /// <summary>
    /// Gets a user by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <returns>The <see cref="User"/> if found; otherwise, null.</returns>
    Task<User?> GetByIdAsync(string id);

    /// <summary>
    /// Creates a new user asynchronously with the specified password.
    /// </summary>
    /// <param name="request">The create user request containing username, email, description, admin flag, and password.</param>
    /// <returns>The created <see cref="User"/>.</returns>
    Task<User> CreateAsync(CreateUserRequest request);

    /// <summary>
    /// Creates a new user asynchronously with the specified password and role.
    /// </summary>
    /// <param name="request">The create user request containing username, email, description, admin flag, and password.</param>
    /// <param name="roleName">The name of the role to assign to the user.</param>
    /// <returns>The created <see cref="User"/>.</returns>
    Task<User> CreateAsync(CreateUserRequest request, string roleName);

    /// <summary>
    /// Updates an existing user asynchronously.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="request">The update user request containing new description and admin flag.</param>
    /// <returns>True if the user was updated; otherwise, false.</returns>
    Task<bool> UpdateAsync(string id, UpdateUserRequest request);

    /// <summary>
    /// Deletes a user asynchronously.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <returns>True if the user was deleted; otherwise, false.</returns>
    Task<bool> DeleteAsync(string id);

    /// <summary>
    /// Authenticates a user with username and password asynchronously.
    /// Updates the <see cref="User.LastLogin"/> timestamp on successful login.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="password">The password.</param>
    /// <returns>The authenticated <see cref="User"/> if successful; otherwise, null.</returns>
    Task<User?> LoginAsync(string username, string password);

    /// <summary>
    /// Logs out the current user asynchronously.
    /// </summary>
    Task LogoutAsync();

    /// <summary>
    /// Generates a password reset token for the specified username asynchronously.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <returns>The password reset token if the user exists; otherwise, null.</returns>
    Task<string?> ResetPasswordRequestAsync(string username);

    /// <summary>
    /// Confirms a password reset using a token and sets a new password asynchronously.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <param name="token">The password reset token.</param>
    /// <param name="newPassword">The new password.</param>
    /// <returns>True if the password reset succeeded; otherwise, false.</returns>
    Task<bool> ResetPasswordConfirmationAsync(string username, string token, string newPassword);

    /// <summary>
    /// Assigns a role to a user asynchronously.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="roleName">The role name to assign.</param>
    /// <returns>True if the role was assigned; otherwise, false.</returns>
    Task<bool> AssignRoleAsync(string userId, string roleName);

    /// <summary>
    /// Removes a role from a user asynchronously.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="roleName">The role name to remove.</param>
    /// <returns>True if the role was removed; otherwise, false.</returns>
    Task<bool> RemoveRoleAsync(string userId, string roleName);

    /// <summary>
    /// Assigns a permission (claim) to a user asynchronously.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="permission">The permission to assign.</param>
    /// <returns>True if the permission was assigned; otherwise, false.</returns>
    Task<bool> AssignPermissionAsync(string userId, string permission);

    /// <summary>
    /// Removes a permission (claim) from a user asynchronously.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="permission">The permission to remove.</param>
    /// <returns>True if the permission was removed; otherwise, false.</returns>
    Task<bool> RemovePermissionAsync(string userId, string permission);

    /// <summary>
    /// Gets all roles and permissions for a user asynchronously.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The user's roles and permissions.</returns>
    Task<UserRolesAndPermissionsResponse?> GetUserRolesAndPermissionsAsync(string userId);
}