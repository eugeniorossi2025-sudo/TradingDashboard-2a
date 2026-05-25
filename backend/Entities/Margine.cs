using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;

/// <summary>
/// Margin time-series written by the Decisore via InsertMargine SP (dbo.Margini).
/// Read-only from WebApi — never written here.
/// </summary>
[Table("Margini")]
public class Margine
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("Margine")]
    public decimal? MargineValue { get; set; }

    [Column("Data")]
    public DateTime? Data { get; set; }
}
