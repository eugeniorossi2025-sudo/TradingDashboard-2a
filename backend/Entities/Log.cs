using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;

/// <summary>
/// Represents a log entry in the system (JsonLog).
/// </summary>
[Table("Logs")]
public class Log
{
    /// <summary>
    /// Gets or sets the unique identifier for the log entry.
    /// </summary>
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the log entry was created.
    /// </summary>
    [Column("DateTime")]
    public DateTime DateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// Gets or sets the margin value associated with the log entry.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Margine { get; set; }

    /// <summary>
    /// Gets or sets the notes or description for the log entry.
    /// </summary>
    [Column("Notes")]
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the JSON data associated with the log entry.
    /// </summary>
    [Column("Json")]
    public string? Json { get; set; }
}