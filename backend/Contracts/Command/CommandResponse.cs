// Contracts/Command/CommandResponse.cs

namespace Contracts.Command;

/// <summary>
/// Represents a command response.
/// </summary>
public class CommandResponse
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the command type ID.
    /// </summary>
    public int IdCommand { get; set; }

    /// <summary>
    /// Gets or sets the command type name.
    /// </summary>
    public string CommandTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the PC/Account name.
    /// </summary>
    public string? Pc { get; set; }

    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    public int IdUser { get; set; }

    /// <summary>
    /// Gets or sets the creation date and time.
    /// </summary>
    public DateTime DateTime { get; set; }
}