namespace Contracts.Auth;

/// <summary>
/// DTO for user login request
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Username of the user
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password of the user
    /// </summary>
    public string Password { get; set; } = string.Empty;
}