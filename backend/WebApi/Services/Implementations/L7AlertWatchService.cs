using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;

namespace WebApi.Services.Implementations;

/// <summary>
/// Detects transitions to martingale level L7 on live tables and notifies administrators.
/// </summary>
public sealed class L7AlertWatchService : IL7AlertWatchService
{
    private const int L7Threshold = 7;

    private static readonly ConcurrentDictionary<string, int> LastLevels = new(StringComparer.OrdinalIgnoreCase);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<L7AlertWatchService> _logger;

    public L7AlertWatchService(IServiceScopeFactory scopeFactory, ILogger<L7AlertWatchService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task CheckAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var pushNotificationService = scope.ServiceProvider.GetRequiredService<IPushNotificationService>();

        var rows = await context.PcCurrentStatuses
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var currentKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            var key = BuildKey(row.Computer, row.Tavolo);
            currentKeys.Add(key);

            var currentLevel = row.ColpoMartingala;

            if (!LastLevels.TryGetValue(key, out var previousLevel))
            {
                LastLevels[key] = currentLevel;
                continue;
            }

            if (previousLevel < L7Threshold && currentLevel >= L7Threshold)
            {
                try
                {
                    var sent = await pushNotificationService.SendAdminBotLevelAlertAsync(
                        row.Computer,
                        row.Tavolo,
                        currentLevel,
                        row.Margine,
                        cancellationToken);

                    if (sent > 0)
                    {
                        _logger.LogInformation(
                            "L7 admin push sent to {Count} subscription(s) for {Computer} table {Tavolo} (level {Level})",
                            sent,
                            row.Computer,
                            row.Tavolo,
                            currentLevel);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "L7 admin push failed for {Computer} table {Tavolo}", row.Computer, row.Tavolo);
                }
            }

            LastLevels[key] = currentLevel;
        }

        foreach (var key in LastLevels.Keys.Where(staleKey => !currentKeys.Contains(staleKey)).ToList())
            LastLevels.TryRemove(key, out _);
    }

    private static string BuildKey(string computer, string? tavolo) =>
        $"{computer}|{tavolo?.Trim() ?? string.Empty}";
}
