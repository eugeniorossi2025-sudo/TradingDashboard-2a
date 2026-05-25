using Contracts.Command;
using Entities;

namespace WebApi.Extensions.Mapping;

public static class CommandMappingExtensions
{
    public static Command MapToEntity(this CreateCommandRequest request)
    {
        return new Command
        {
            IdCommand = request.IdCommand,
            Pc = request.Pc,
            IdUser = request.IdUser,
            DateTime = DateTime.Now,
            BitSent = false
        };
    }

    public static void UpdateFromRequest(this Command command, UpdateCommandRequest request)
    {
        command.IdCommand = request.IdCommand;
        if (request.Pc != null) command.Pc = request.Pc;
    }

    public static CommandResponse ToContract(this Command command)
    {
        return new CommandResponse
        {
            Id = (int)command.Id,
            IdCommand = (int)(command.IdCommand ?? 0),
            CommandTypeName = GetCommandTypeName((int)(command.IdCommand ?? 0)),
            Pc = command.Pc,
            IdUser = command.IdUser ?? 0,
            DateTime = command.DateTime ?? DateTime.MinValue
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
