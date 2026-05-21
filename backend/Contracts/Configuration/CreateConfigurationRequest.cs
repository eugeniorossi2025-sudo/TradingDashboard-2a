namespace Contracts.Configuration;

/// <summary>
/// Represents a request to create a new configuration.
/// </summary>
public class CreateConfigurationRequest(string key, string description, int pos, string value)
{
    /// <summary>
    /// Gets or sets the configuration key.
    /// </summary>
    public string Key { get; set; } = key;

    /// <summary>
    /// Gets or sets the configuration description.
    /// </summary>
    public string Description { get; set; } = description;

    /// <summary>
    /// Gets or sets the position or order of the configuration.
    /// </summary>
    public int Pos { get; set; } = pos;

    /// <summary>
    /// Gets or sets the configuration value.
    /// </summary>
    public string Value { get; set; } = value;
}