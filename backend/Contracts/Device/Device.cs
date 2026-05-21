namespace Contracts.Device;

/// <summary>
/// Represents a PC/Device data transfer object.
/// </summary>
public class Device
{
    /// <summary>
    /// Gets or sets the PC identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the PC name/title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the PC status (0=Offline, 1=Online).
    /// </summary>
    public int Stato { get; set; }

    /// <summary>
    /// Amount of single pc
    /// </summary>
    public decimal Amount { get; set; }


    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTime? LastUpdate { get; set; }
}