using Contracts.Auth;

namespace WebApi.Services;

/// <summary>
/// Interface for authentication operations.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user and generates a JWT token.
    /// </summary>
    Task<LoginResponse?> LoginAsync(string username, string password);

    /// <summary>
    /// Logs out the current user.
    /// </summary>
    Task LogoutAsync();

    /// <summary>
    /// Generates a password reset token for the specified email.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <returns>The reset token if user exists; otherwise, null.</returns>
    Task<string?> GeneratePasswordResetTokenAsync(string email);

    /// <summary>
    /// Resets the password using a token and new password.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="token">The password reset token.</param>
    /// <param name="newPassword">The new password.</param>
    /// <returns>True if password was reset successfully; otherwise, false.</returns>
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
}