// Contracts/Command/UpdateCommandRequest.cs

namespace Contracts.Command;

/// <summary>
/// Represents a request to update an existing command.
/// </summary>
public class UpdateCommandRequest
{
    /// <summary>
    /// Gets or sets the command type ID.
    /// </summary>
    public int IdCommand { get; set; }

    /// <summary>
    /// Gets or sets the PC/Account name.
    /// </summary>
    public string? Pc { get; set; }
}