namespace Contracts.Configuration;

/// <summary>
/// Represents a request to update an existing configuration.
/// </summary>
public class UpdateConfigurationRequest
{
    /// <summary>
    /// Gets or sets the configuration description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the position or order of the configuration.
    /// </summary>
    public int? Pos { get; set; }

    /// <summary>
    /// Gets or sets the configuration value.
    /// </summary>
    public string? Value { get; set; }
}