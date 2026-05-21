using System.ComponentModel.DataAnnotations;

namespace Contracts.User;

/// <summary>
/// Request to assign a permission to a user.
/// </summary>
public class AssignPermissionRequest
{
    /// <summary>
    /// The permission to assign (e.g., "user.read", "bot.manage", "config.manage").
    /// </summary>
    [Required]
    public string Permission { get; set; } = string.Empty;
}
