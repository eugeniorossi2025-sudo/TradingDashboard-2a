namespace Contracts.Auth;

/// <summary>
/// DTO for user login response containing JWT token
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Generated JWT token
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Expiration date/time of the token
    /// </summary>
    public DateTime Expiration { get; set; }

    public LoginResponse()
    {
    }

    public LoginResponse(string token, DateTime expiration)
    {
        Token = token;
        Expiration = expiration;
    }
}