namespace Contracts.User;

/// <summary>
/// Represents a request to update an existing user.
/// </summary>
public class UpdateUserRequest
{
    /// <summary>
    /// Gets or sets the user description or profile information.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user has administrator privileges.
    /// </summary>
    public bool? IsAdmin { get; set; }

    /// <summary>
    /// Gets or sets the date and time of the user's last login.
    /// </summary>
    public DateTime? LastLogin { get; set; }
}