// Contracts/Command/CreateCommandRequest.cs

namespace Contracts.Command;

/// <summary>
/// Represents a request to create a new command.
/// </summary>
public class CreateCommandRequest
{
    /// <summary>
    /// Gets or sets the command type ID (1=StopPc, 2=AzzeraMartingala, 3=StartPc).
    /// </summary>
    public int IdCommand { get; set; }

    /// <summary>
    /// Gets or sets the PC/Account name.
    /// </summary>
    public string? Pc { get; set; }

    /// <summary>
    /// Gets or sets the user ID who issued the command.
    /// </summary>
    public int IdUser { get; set; }
}