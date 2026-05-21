// WebApi/Models/Value.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;

/// <summary>
/// Represents a telemetry value from bot operations.
/// </summary>
[Table("Values")]
public class Values
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the timestamp key (format: yyyyMMddHHmmssfff).
    /// </summary>
    [Column("Key")]
    public string Key { get; set; }

    /// <summary>
    /// Gets or sets the parameter description (e.g., "COMPUTER", "TAVOLO", "MARGINE").
    /// </summary>
    [Column("Description")]
    [MaxLength(255)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the parameter value.
    /// </summary>
    [Column("Value")]
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the user ID who created this value.
    /// </summary>
    [Column("Id_User")]
    public int IdUser { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this value was created.
    /// </summary>
    [Column("DateTime")]
    public DateTime DateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// Gets or sets the bot account name.
    /// </summary>
    [Column("ACCOUNT")]
    [MaxLength(100)]
    public string? Account { get; set; }

    /// <summary>
    /// Gets or sets the table ID.
    /// </summary>
    [Column("TAVOLO")]
    public int? Tavolo { get; set; }

    /// <summary>
    /// Gets or sets the remaining cards in deck.
    /// </summary>
    [Column("MAZZO")]
    public int? Mazzo { get; set; }

    /// <summary>
    /// Gets or sets the profit margin.
    /// </summary>
    [Column("MARGINE")]
    [MaxLength(20)]
    public decimal? Margine { get; set; }

    /// <summary>
    /// Gets or sets the hourly average.
    /// </summary>
    [Column("MEDIA_ORA")]
    [MaxLength(20)]
    public decimal? MediaOra { get; set; }

    /// <summary>
    /// Gets or sets the bot status.
    /// </summary>
    [Column("STATO")]
    [MaxLength(50)]
    public string? Stato { get; set; }

    /// <summary>
    /// Gets or sets the color indicator (ROSSO/GIALLO/VERDE).
    /// </summary>
    [Column("COLORE")]
    [MaxLength(50)]
    public string? Colore { get; set; }

    /// <summary>
    /// Gets or sets the martingale level.
    /// </summary>
    [Column("COLPO_MARTINGALA")]
    public int? ColpoMartingala { get; set; }

    /// <summary>
    /// Gets or sets the evaluation from proactive engine.
    /// </summary>
    [Column("VALUTAZIONE")]
    public string? Valutazione { get; set; }

    /// <summary>
    /// Gets or sets the decision reason.
    /// </summary>
    [Column("REASON")]
    public string? Reason { get; set; }

    /// <summary>
    /// Gets or sets the prediction.
    /// </summary>
    [Column("PREDICTION")]
    [MaxLength(100)]
    public string? Prediction { get; set; }

    /// <summary>
    /// Gets or sets the hand result (P=Pari, B=Banco, T=Tie).
    /// </summary>
    [Column("PBT")]
    [MaxLength(1)]
    public string? Pbt { get; set; }

    /// <summary>
    /// Gets or sets the elapsed time (format: HH:MM).
    /// </summary>
    [Column("TEMPO")]
    [MaxLength(10)]
    public string? Tempo { get; set; }

    /// <summary>
    /// Navigation property to User.
    /// </summary>
    [ForeignKey("IdUser")]
    public User? User { get; set; }
}