using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;

/// <summary>
/// Session statistics written by the Decisore after each reset (dbo.Statistiche).
/// Read-only from WebApi — never written here.
/// </summary>
[Table("Statistiche")]
public class Statistica
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Column("DATA_INIZIO")]
    public DateTime DataInizio { get; set; }

    [Column("DATA_FINE")]
    public DateTime? DataFine { get; set; }

    [Column("MARGINE_TOT")]
    public decimal MargineTot { get; set; }

    [Column("MARGINE_MIN")]
    public decimal MargineMin { get; set; }

    [Column("MARGINE_MAX")]
    public decimal MargineMax { get; set; }

    [Column("ELAPSED")]
    public decimal Elapsed { get; set; }

    [Column("TELEMETRY")]
    [MaxLength(4000)]
    public string? Telemetry { get; set; }
}
