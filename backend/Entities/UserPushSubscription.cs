using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;

[Table("UserPushSubscriptions")]
public class UserPushSubscription
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    public int UserId { get; set; }

    [MaxLength(2048)]
    public string Endpoint { get; set; } = string.Empty;

    [MaxLength(512)]
    public string P256dh { get; set; } = string.Empty;

    [MaxLength(256)]
    public string Auth { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string? UserAgent { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime LastSeenAtUtc { get; set; } = DateTime.UtcNow;

    public bool Enabled { get; set; } = true;

    public User? User { get; set; }
}
