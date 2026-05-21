// Entities/Configuration.cs - UPDATED

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;

/// <summary>
/// Represents a configuration setting in the system.
/// </summary>
[Table("Configurations")]
public class Configuration
{
    /// <summary>
    /// Gets or sets the unique identifier for the configuration.
    /// </summary>
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the configuration key (e.g., "CHECK_PC_OFF", "BALANCE_DIVIDER").
    /// </summary>
    [Column("Key")]
    [Required]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the configuration.
    /// </summary>
    [Column("Description")]
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the position/order of the configuration.
    /// </summary>
    [Column("Pos")]
    public int Pos { get; set; }

    /// <summary>
    /// Gets or sets the value of the configuration.
    /// </summary>
    [Column("Value")]
    public string? Value { get; set; }
}