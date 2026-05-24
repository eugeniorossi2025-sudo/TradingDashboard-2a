using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;

/// <summary>
/// Financial sample for a mission session. It stores accounting margin points only.
/// </summary>
[Table("MissionMarginSamples")]
public class MissionMarginSample
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    public int SessionId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalMargin { get; set; }

    public int ActiveTables { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal VmCurrent { get; set; }

    [MaxLength(32)]
    public string RuntimeMode { get; set; } = "Production";

    public MissionSession? Session { get; set; }
}
