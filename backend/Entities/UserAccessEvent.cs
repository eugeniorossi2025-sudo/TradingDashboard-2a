using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;

[Table("UserAccessEvents")]
public class UserAccessEvent
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    public int? UserId { get; set; }

    [MaxLength(256)]
    public string? Username { get; set; }

    [MaxLength(32)]
    public string EventType { get; set; } = "PAGE_VIEW";

    [MaxLength(128)]
    public string? IpAddress { get; set; }

    [MaxLength(512)]
    public string? Page { get; set; }

    [MaxLength(1024)]
    public string? UserAgent { get; set; }

    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
