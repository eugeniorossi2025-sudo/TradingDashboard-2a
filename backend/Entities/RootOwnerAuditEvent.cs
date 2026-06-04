using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;

[Table("RootOwnerAuditEvents")]
public class RootOwnerAuditEvent
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    public int? ActorUserId { get; set; }

    [MaxLength(256)]
    public string? ActorUsername { get; set; }

    [MaxLength(64)]
    public string Action { get; set; } = string.Empty;

    public DateTime OccurredAtUtc { get; set; }

    [MaxLength(128)]
    public string? IpAddress { get; set; }

    [MaxLength(1024)]
    public string? UserAgent { get; set; }

    [MaxLength(32)]
    public string Outcome { get; set; } = "OK";

    [MaxLength(512)]
    public string? Reason { get; set; }

    [MaxLength(4000)]
    public string? DetailsJson { get; set; }
}
