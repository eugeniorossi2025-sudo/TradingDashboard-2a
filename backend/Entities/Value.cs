using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;

/// <summary>
/// Production Values table (legacy schema, 6 columns).
/// </summary>
[Table("Values")]
public class Values
{
    [Key]
    [Column("ID")]
    public decimal Id { get; set; }

    [Column("Key")]
    public decimal? Key { get; set; }

    [Column("Description")]
    [MaxLength(50)]
    public string? Description { get; set; }

    [Column("Value")]
    [MaxLength(50)]
    public string? Value { get; set; }

    [Column("ID_User")]
    public int? IdUser { get; set; }

    [Column("Datetime")]
    public DateTime? DateTime { get; set; }

    [ForeignKey(nameof(IdUser))]
    public User? User { get; set; }
}
