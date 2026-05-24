using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;

[Table("UserNotificationSettings")]
public class UserNotificationSetting
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    public int UserId { get; set; }

    [MaxLength(256)]
    public string? NotificationEmail { get; set; }

    public bool Enabled { get; set; } = true;

    public bool Mission { get; set; } = true;

    public bool System { get; set; } = true;

    public bool Errors { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
