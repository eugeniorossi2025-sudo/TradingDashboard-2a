using Microsoft.AspNetCore.SignalR;
using WebApi.Hubs;

namespace WebApi.Services.Implementations;

/// <summary>
/// Background service that periodically sends dashboard updates to connected clients.
/// </summary>
public class DashboardUpdateService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<DashboardHub> _hubContext;
    private readonly ILogger<DashboardUpdateService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardUpdateService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="hubContext">The SignalR hub context.</param>
    /// <param name="logger">The logger.</param>
    public DashboardUpdateService(
        IServiceProvider serviceProvider,
        IHubContext<DashboardHub> hubContext,
        ILogger<DashboardUpdateService> logger)
    {
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Executes the background service, sending updates every 1.5 seconds.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var missionLifecycleService = scope.ServiceProvider.GetRequiredService<IMissionLifecycleService>();
                    await missionLifecycleService.ObserveLiveStateAsync(stoppingToken);

                    var dashboardService = scope.ServiceProvider.GetRequiredService<IDashboardService>();
                    var dashboardData = await dashboardService.GetDashboardDataAsync();

                    await _hubContext.Clients.Group("Dashboard")
                        .SendAsync("ReceiveDashboardUpdate", dashboardData, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending dashboard update");
            }

            await Task.Delay(1500, stoppingToken); // Update every 1.5 seconds
        }
    }
}