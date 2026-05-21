namespace Contracts.User;

/// <summary>
/// Response containing user's roles and permissions.
/// </summary>
public class UserRolesAndPermissionsResponse
{
    /// <summary>
    /// User identifier.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Username.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// List of roles assigned to the user.
    /// </summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>
    /// List of permissions assigned to the user.
    /// </summary>
    public List<string> Permissions { get; set; } = new();

    /// <summary>
    /// Whether the user has admin privileges.
    /// </summary>
    public bool IsAdmin { get; set; }
}
