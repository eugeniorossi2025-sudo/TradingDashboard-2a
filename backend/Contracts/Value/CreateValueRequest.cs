// Contracts/Value/CreateValueRequest.cs

namespace Contracts.Value;

/// <summary>
/// Represents a request to create a new telemetry value.
/// </summary>
public class CreateValueRequest
{
    /// <summary>
    /// Gets or sets the timestamp key (format: yyyyMMddHHmmssfff).
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Gets or sets the parameter description (e.g., "COMPUTER", "TAVOLO", "MARGINE").
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the parameter value.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the user ID who created this value.
    /// </summary>
    public int IdUser { get; set; }

    /// <summary>
    /// Gets or sets the bot account name.
    /// </summary>
    public string? Account { get; set; }

    /// <summary>
    /// Gets or sets the table ID.
    /// </summary>
    public int? Tavolo { get; set; }

    /// <summary>
    /// Gets or sets the remaining cards in deck.
    /// </summary>
    public int? Mazzo { get; set; }

    /// <summary>
    /// Gets or sets the profit margin.
    /// </summary>
    public decimal? Margine { get; set; }

    /// <summary>
    /// Gets or sets the hourly average.
    /// </summary>
    public decimal? MediaOra { get; set; }

    /// <summary>
    /// Gets or sets the bot status.
    /// </summary>
    public string? Stato { get; set; }

    /// <summary>
    /// Gets or sets the color indicator (ROSSO/GIALLO/VERDE).
    /// </summary>
    public string? Colore { get; set; }

    /// <summary>
    /// Gets or sets the martingale level.
    /// </summary>
    public int? ColpoMartingala { get; set; }

    /// <summary>
    /// Gets or sets the evaluation from proactive engine.
    /// </summary>
    public string? Valutazione { get; set; }

    /// <summary>
    /// Gets or sets the decision reason.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Gets or sets the prediction.
    /// </summary>
    public string? Prediction { get; set; }

    /// <summary>
    /// Gets or sets the hand result (P=Pari, B=Banco, T=Tie).
    /// </summary>
    public string? Pbt { get; set; }

    /// <summary>
    /// Gets or sets the elapsed time (format: HH:MM).
    /// </summary>
    public string? Tempo { get; set; }
}