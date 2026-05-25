using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Entities;

/// <summary>
/// Represents a user in the system.
/// </summary>
[Table("Users_v2")]
public class User : IdentityUser<int>
{
    /// <summary>
    /// Gets or sets the description or profile information for the user.
    /// </summary>
    [Column("Description")]
    [Obsolete("Use Identity Roles instead")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user has administrator privileges.
    /// </summary>
    [Column("Admin")]
    public bool Admin { get; set; }

    /// <summary>
    /// Gets or sets the date and time of the user's last login.
    /// </summary>
    [Column("LastLogin")]
    public DateTime? LastLogin { get; set; }
}