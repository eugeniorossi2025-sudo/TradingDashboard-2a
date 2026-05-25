using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;

/// <summary>
/// Production Configurations table uses K as primary key (legacy schema).
/// </summary>
[Table("Configurations")]
public class Configuration
{
    [Key]
    [Column("K")]
    [MaxLength(50)]
    public string Key { get; set; } = string.Empty;

    [Column("Description")]
    [MaxLength(500)]
    public string? Description { get; set; }

    [Column("Pos")]
    public int? Pos { get; set; }

    [Column("Value")]
    [MaxLength(4000)]
    public string? Value { get; set; }
}
