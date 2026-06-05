using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;

/// <summary>
/// One-shot operator ActionCode override for Control Room (consumed on next decide per PC).
/// </summary>
[Table("ControlRoomCommandOverrides")]
public class ControlRoomCommandOverride
{
    [Key]
    [MaxLength(50)]
    [Column("PC")]
    public string Pc { get; set; } = string.Empty;

    public int ActionCode { get; set; }

    [MaxLength(32)]
    public string CommandType { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public int? CreatedByUserId { get; set; }
}
