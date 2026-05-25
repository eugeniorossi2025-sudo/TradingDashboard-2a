using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;

/// <summary>
/// Production PC list table (legacy schema: dbo.Pc).
/// </summary>
[Table("Pc")]
public class Device
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("NAME")]
    [MaxLength(50)]
    public string? Name { get; set; }

    [Column("TOTAL")]
    public decimal? Total { get; set; }
}
