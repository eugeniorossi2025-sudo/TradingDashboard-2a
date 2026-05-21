namespace Contracts.Device;

/// <summary>
/// Represents the request to create a new PC/Device.
/// </summary>
public class CreateDeviceRequest
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
    /// Gets or sets the last update timestamp.
    /// </summary>
    public int Amount { get; set; }
}