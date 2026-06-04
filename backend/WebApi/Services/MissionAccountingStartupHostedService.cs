namespace WebApi.Services;

/// <summary>
/// At application startup, recovers stale open mission rows so accounting invariant holds (at most one Completed=false).
/// </summary>
public sealed class MissionAccountingStartupHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MissionAccountingStartupHostedService> _logger;

    public MissionAccountingStartupHostedService(
        IServiceProvider serviceProvider,
        ILogger<MissionAccountingStartupHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var missionLifecycle = scope.ServiceProvider.GetRequiredService<IMissionLifecycleService>();
            await missionLifecycle.EnsureAccountingInvariantAtStartupAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Mission accounting startup invariant check failed");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
