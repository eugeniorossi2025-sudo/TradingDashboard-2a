namespace Contracts.Log;

/// <summary>
/// Represents a request to create a new log entry.
/// </summary>
public class CreateLogRequest
{
    /// <summary>
    /// Gets or sets the date and time of the log entry.
    /// </summary>
    public DateTime DateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// Gets or sets the margin value associated with the log entry.
    /// </summary>
    public decimal Margine { get; set; }

    /// <summary>
    /// Gets or sets the notes or description for the log entry.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the JSON data associated with the log entry.
    /// </summary>
    public string? Json { get; set; }
}