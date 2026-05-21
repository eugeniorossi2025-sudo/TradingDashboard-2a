namespace Contracts.User;

/// <summary>
/// Represents a request to create a new user.
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Gets or sets the user description or profile information.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user has administrator privileges.
    /// </summary>
    public bool IsAdmin { get; set; }

    /// <summary>
    /// Gets or sets the password for the user account.
    /// </summary>
    public required string Password { get; set; }
}