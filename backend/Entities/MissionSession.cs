using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;

/// <summary>
/// Accounting mission/session used by financial reports. Runtime logs must not be mixed into this table.
/// </summary>
[Table("MissionSessions")]
public class MissionSession
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [MaxLength(128)]
    public string? MissionKey { get; set; }

    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    public DateTime? EndTime { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalMargin { get; set; }

    public int RealHandsCount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? LastTotalMarginForRealHands { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal GlobalTarget { get; set; }

    public int ActiveTables { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal KFactor { get; set; } = 1.0m;

    [MaxLength(32)]
    public string RuntimeMode { get; set; } = "Production";

    public bool Completed { get; set; }

    public DateTime? ReportPublishedAt { get; set; }

    [MaxLength(128)]
    public string? FinalizationReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<MissionMarginSample> Samples { get; set; } = new List<MissionMarginSample>();
}
