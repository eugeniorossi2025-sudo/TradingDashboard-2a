// WebApi/Extensions/Mapping/CommandMappingExtensions.cs

using Contracts.Command;
using Entities;

namespace WebApi.Extensions.Mapping;

/// <summary>
/// Extension methods for mapping Command entities.
/// </summary>
public static class CommandMappingExtensions
{
    /// <summary>
    /// Maps a CreateCommandRequest to a Command entity.
    /// </summary>
    public static Command MapToEntity(this CreateCommandRequest request)
    {
        return new Command
        {
            IdCommand = request.IdCommand,
            Pc = request.Pc,
            IdUser = request.IdUser,
            DateTime = DateTime.Now
        };
    }

    /// <summary>
    /// Updates an existing Command entity from UpdateCommandRequest.
    /// </summary>
    public static void UpdateFromRequest(this Command command, UpdateCommandRequest request)
    {
        command.IdCommand = request.IdCommand;
        if (request.Pc != null) command.Pc = request.Pc;
    }

    /// <summary>
    /// Maps a Command entity to CommandResponse.
    /// </summary>
    public static CommandResponse ToContract(this Command command)
    {
        return new CommandResponse
        {
            Id = command.Id,
            IdCommand = command.IdCommand,
            CommandTypeName = GetCommandTypeName(command.IdCommand),
            Pc = command.Pc,
            IdUser = command.IdUser,
            DateTime = command.DateTime
        };
    }

    private static string GetCommandTypeName(int idCommand)
    {
        return idCommand switch
        {
            1 => "StopPc",
            2 => "AzzeraMartingala",
            3 => "StartPc",
            _ => "Unknown"
        };
    }
}