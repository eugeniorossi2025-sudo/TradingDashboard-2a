// WebApi/Services/Implementations/CommandService.cs

using Contracts.Command;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Extensions.Mapping;

namespace WebApi.Services.Implementations;

/// <summary>
/// Service for managing bot commands.
/// </summary>
public class CommandService : ICommandService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CommandService> _logger;

    public CommandService(AppDbContext context, ILogger<CommandService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CommandResponse> CreateAsync(CreateCommandRequest request)
    {
        var command = request.MapToEntity();

        _context.Commands.Add(command);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Command created - ID: {Id}, Type: {Type} ({TypeName}), PC: {Pc}, User: {UserId}",
            command.Id, command.IdCommand, command.ToContract().CommandTypeName, command.Pc, command.IdUser);

        return command.ToContract();
    }

    public async Task<CommandResponse?> GetByIdAsync(int id)
    {
        _logger.LogDebug("Getting command by ID: {Id}", id);

        var command = await _context.Commands.FindAsync(id);

        if (command == null)
        {
            _logger.LogWarning("Command not found - ID: {Id}", id);
            return null;
        }

        return command.ToContract();
    }

    public async Task<IEnumerable<CommandResponse>> GetAllAsync(int page = 1, int pageSize = 50)
    {
        _logger.LogDebug("Getting commands - Page: {Page}, PageSize: {PageSize}", page, pageSize);

        var commands = await _context.Commands
            .OrderByDescending(c => c.DateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} commands", commands.Count);

        return commands.Select(c => c.ToContract());
    }

    public async Task<IEnumerable<CommandResponse>> GetByPcAsync(string pc, int limit = 50)
    {
        _logger.LogDebug("Getting commands by PC: {Pc}, Limit: {Limit}", pc, limit);

        var commands = await _context.Commands
            .Where(c => c.Pc == pc)
            .OrderByDescending(c => c.DateTime)
            .Take(limit)
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} commands for PC {Pc}", commands.Count, pc);

        return commands.Select(c => c.ToContract());
    }

    public async Task<IEnumerable<CommandResponse>> GetPendingByPcAsync(string pc)
    {
        var fiveMinutesAgo = DateTime.Now.AddMinutes(-5);

        _logger.LogDebug("Getting pending commands for PC: {Pc} since {DateTime}", pc, fiveMinutesAgo);

        var commands = await _context.Commands
            .Where(c => c.Pc == pc && c.DateTime >= fiveMinutesAgo)
            .OrderByDescending(c => c.DateTime)
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} pending commands for PC {Pc}", commands.Count, pc);

        return commands.Select(c => c.ToContract());
    }

    public async Task<IEnumerable<CommandResponse>> GetByUserAsync(int userId, int limit = 50)
    {
        _logger.LogDebug("Getting commands by user: {UserId}, Limit: {Limit}", userId, limit);

        var commands = await _context.Commands
            .Where(c => c.IdUser == userId)
            .OrderByDescending(c => c.DateTime)
            .Take(limit)
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} commands for user {UserId}", commands.Count, userId);

        return commands.Select(c => c.ToContract());
    }

    public async Task<CommandResponse> StartPcAsync(string pc, int userId)
    {
        _logger.LogInformation("Issuing StartPc command - PC: {Pc}, User: {UserId}", pc, userId);

        var request = new CreateCommandRequest
        {
            IdCommand = 3, // StartPc
            Pc = pc,
            IdUser = userId
        };

        return await CreateAsync(request);
    }

    public async Task<CommandResponse> StopPcAsync(string pc, int userId)
    {
        _logger.LogInformation("Issuing StopPc command - PC: {Pc}, User: {UserId}", pc, userId);

        var request = new CreateCommandRequest
        {
            IdCommand = 1, // StopPc
            Pc = pc,
            IdUser = userId
        };

        return await CreateAsync(request);
    }

    public async Task<CommandResponse> ResetMartingaleAsync(string pc, int userId)
    {
        _logger.LogInformation("Issuing ResetMartingale command - PC: {Pc}, User: {UserId}", pc, userId);

        var request = new CreateCommandRequest
        {
            IdCommand = 2, // AzzeraMartingala
            Pc = pc,
            IdUser = userId
        };

        return await CreateAsync(request);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogDebug("Deleting command ID: {Id}", id);

        var command = await _context.Commands.FindAsync(id);
        if (command == null)
        {
            _logger.LogWarning("Command not found for deletion - ID: {Id}", id);
            return false;
        }

        _context.Commands.Remove(command);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Command deleted - ID: {Id}, Type: {Type}, PC: {Pc}",
            id, command.IdCommand, command.Pc);

        return true;
    }
}