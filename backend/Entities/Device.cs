// Entities/Device.cs - UPDATED to match PC table

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;

/// <summary>
/// Represents a PC/Bot machine in the system.
/// </summary>
[Table("PC")]
public class Device
{
    /// <summary>
    /// Gets or sets the PC identifier (primary key).
    /// </summary>
    [Key]
    [Column("PC")]
    [MaxLength(100)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the PC name/title.
    /// </summary>
    [Column("Title")]
    [MaxLength(255)]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the PC status (0=Offline, 1=Online).
    /// </summary>
    [Column("STATO")]
    public int Stato { get; set; }


    /// <summary>
    /// Gets or sets the PC status (0=Offline, 1=Online).
    /// </summary>
    [Column("IMPORTO")]
    public int Amount { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    [Column("LAST_UPDATE")]
    public DateTime? LastUpdate { get; set; }
}