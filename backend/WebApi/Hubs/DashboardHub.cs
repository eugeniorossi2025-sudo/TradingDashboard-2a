using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WebApi.Hubs;

/// <summary>
/// SignalR Hub for real-time dashboard updates.
/// </summary>
[AllowAnonymous]
public class DashboardHub : Hub
{
    private readonly ILogger<DashboardHub> _logger;

    public DashboardHub(ILogger<DashboardHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects to the hub.
    /// </summary>
    /// <returns>A completed task.</returns>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("id")?.Value ?? "anonymous";
        _logger.LogInformation($"Client connected: {Context.ConnectionId}, User: {userId}");

        await Groups.AddToGroupAsync(Context.ConnectionId, "Dashboard");
        await Clients.Caller.SendAsync("Connected",
            new { connectionId = Context.ConnectionId, message = "Successfully connected to Dashboard Hub" });
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub.
    /// </summary>
    /// <param name="exception">The exception that caused the disconnect.</param>
    /// <returns>A completed task.</returns>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Dashboard");
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Sends dashboard update to all connected clients.
    /// </summary>
    /// <param name="data">The dashboard data to broadcast.</param>
    /// <returns>A completed task.</returns>
    public async Task SendDashboardUpdate(object data)
    {
        _logger.LogDebug($"Broadcasting dashboard update to Dashboard group");
        await Clients.Group("Dashboard").SendAsync("ReceiveDashboardUpdate", data);
    }

    /// <summary>
    /// Client requests immediate dashboard refresh.
    /// </summary>
    /// <returns>Confirmation message.</returns>
    public async Task RequestDashboardRefresh()
    {
        _logger.LogInformation($"Client {Context.ConnectionId} requested dashboard refresh");
        await Clients.Caller.SendAsync("RefreshRequested", new { message = "Refresh request received" });
    }

    /// <summary>
    /// Test method to verify SignalR connection.
    /// </summary>
    /// <param name="message">Test message.</param>
    /// <returns>Echo response.</returns>
    public async Task Echo(string message)
    {
        _logger.LogInformation($"Echo from {Context.ConnectionId}: {message}");
        await Clients.Caller.SendAsync("EchoResponse", new { original = message, timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Executes a command on the server.
    /// </summary>
    /// <param name="commandName">The name of the command to execute.</param>
    /// <param name="parameters">Optional parameters for the command.</param>
    /// <returns>Command execution result.</returns>
    public async Task<object> ExecuteCommand(string commandName, object? parameters = null)
    {
        _logger.LogInformation($"ExecuteCommand invoked by {Context.ConnectionId}: {commandName}");
        
        try
        {
            // Qui puoi implementare la logica per eseguire comandi specifici
            // Per ora, restituiamo una risposta di successo
            var result = new 
            { 
                success = true, 
                command = commandName, 
                message = "Command received and queued for execution",
                timestamp = DateTime.UtcNow
            };

            // Notifica tutti i client connessi dell'esecuzione del comando
            await Clients.All.SendAsync("CommandExecuted", result);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error executing command {commandName}");
            return new 
            { 
                success = false, 
                command = commandName, 
                error = ex.Message,
                timestamp = DateTime.UtcNow
            };
        }
    }
}