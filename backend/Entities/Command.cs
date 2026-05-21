// WebApi/Models/Command.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;

/// <summary>
/// Represents a user command to control bot operations.
/// </summary>
[Table("Commands")]
public class Command
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the command type ID.
    /// 1 = StopPc, 2 = AzzeraMartingala, 3 = StartPc
    /// </summary>
    [Column("ID_Command")]
    public int IdCommand { get; set; }

    /// <summary>
    /// Gets or sets the PC/Account name.
    /// </summary>
    [Column("PC")]
    [MaxLength(100)]
    public string? Pc { get; set; }

    /// <summary>
    /// Gets or sets the user ID who issued the command.
    /// </summary>
    [Column("ID_User")]
    public int IdUser { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the command was created.
    /// </summary>
    [Column("DateTime")]
    public DateTime DateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// Navigation property to User.
    /// </summary>
    [ForeignKey("IdUser")]
    public User? User { get; set; }
}

/// <summary>
/// Enum for command types.
/// </summary>
public enum CommandType
{
    StopPc = 1,
    AzzeraMartingala = 2,
    StartPc = 3
}