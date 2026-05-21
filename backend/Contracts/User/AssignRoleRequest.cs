using System.ComponentModel.DataAnnotations;

namespace Contracts.User;

/// <summary>
/// Request to assign a role to a user.
/// </summary>
public class AssignRoleRequest
{
    /// <summary>
    /// The name of the role to assign (Admin, User, BotOperator).
    /// </summary>
    [Required]
    public string RoleName { get; set; } = string.Empty;
}
