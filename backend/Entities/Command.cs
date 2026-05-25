using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;

/// <summary>
/// Production Commands table (legacy schema).
/// </summary>
[Table("Commands")]
public class Command
{
    [Key]
    [Column("ID")]
    public decimal Id { get; set; }

    [Column("ID_Command")]
    public decimal? IdCommand { get; set; }

    [Column("PC")]
    [MaxLength(50)]
    public string? Pc { get; set; }

    [Column("ID_User")]
    public int? IdUser { get; set; }

    [Column("Datetime")]
    public DateTime? DateTime { get; set; }

    [Column("Bit_Sent")]
    public bool? BitSent { get; set; }

    [ForeignKey(nameof(IdUser))]
    public User? User { get; set; }
}

public enum CommandType
{
    StopPc = 1,
    AzzeraMartingala = 2,
    StartPc = 3
}
