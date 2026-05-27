using System.Text.Json;
using Entities;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebPush;

namespace WebApi.Services.Implementations;

public class PushNotificationService : IPushNotificationService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(
        AppDbContext context,
        IConfiguration configuration,
        ILogger<PushNotificationService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public PushConfigurationState GetConfigurationState()
    {
        var publicKey = _configuration["Push:VapidPublicKey"];
        return new PushConfigurationState
        {
            Enabled = !string.IsNullOrWhiteSpace(publicKey),
            PublicKey = string.IsNullOrWhiteSpace(publicKey) ? null : publicKey
        };
    }

    public async Task SaveSubscriptionAsync(int userId, PushSubscriptionRequest request, string? userAgent, CancellationToken cancellationToken = default)
    {
        await EnsurePushSchemaAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(request.Endpoint) ||
            string.IsNullOrWhiteSpace(request.Keys.P256dh) ||
            string.IsNullOrWhiteSpace(request.Keys.Auth))
        {
            throw new InvalidOperationException("Subscription push incompleta.");
        }

        var now = DateTime.UtcNow;
        var subscription = await _context.UserPushSubscriptions
            .FirstOrDefaultAsync(row => row.Endpoint == request.Endpoint, cancellationToken);

        if (subscription == null)
        {
            subscription = new UserPushSubscription
            {
                UserId = userId,
                Endpoint = request.Endpoint,
                CreatedAtUtc = now
            };
            _context.UserPushSubscriptions.Add(subscription);
        }

        subscription.UserId = userId;
        subscription.P256dh = request.Keys.P256dh;
        subscription.Auth = request.Keys.Auth;
        subscription.UserAgent = string.IsNullOrWhiteSpace(userAgent) ? null : userAgent[..Math.Min(userAgent.Length, 1024)];
        subscription.Enabled = true;
        subscription.LastSeenAtUtc = now;
        subscription.UpdatedAtUtc = now;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> SendMissionNotificationAsync(int sessionId, string eventType, CancellationToken cancellationToken = default)
    {
        var publicKey = _configuration["Push:VapidPublicKey"];
        var privateKey = _configuration["Push:VapidPrivateKey"];
        var subject = _configuration["Push:Subject"];

        if (string.IsNullOrWhiteSpace(publicKey) ||
            string.IsNullOrWhiteSpace(privateKey) ||
            string.IsNullOrWhiteSpace(subject))
        {
            _logger.LogInformation("Mission push skipped because VAPID is not configured.");
            return 0;
        }

        await EnsurePushSchemaAsync(cancellationToken);

        var session = await _context.MissionSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(row => row.Id == sessionId, cancellationToken);

        if (session == null)
            return 0;

        var subscriptions = await _context.UserPushSubscriptions
            .Where(row => row.Enabled)
            .ToListAsync(cancellationToken);

        if (subscriptions.Count == 0)
            return 0;

        var isStart = string.Equals(eventType, "started", StringComparison.OrdinalIgnoreCase);
        var payload = JsonSerializer.Serialize(new
        {
            title = isStart ? "DASH2A missione avviata" : "DASH2A missione finalizzata",
            body = isStart
                ? $"Missione #{session.Id} {session.RuntimeMode} avviata. Target {session.GlobalTarget:0.00} EUR."
                : $"Missione #{session.Id} {session.RuntimeMode} chiusa. Margine {session.TotalMargin:0.00} EUR.",
            data = new
            {
                url = "/admin/mobile-live",
                sessionId = session.Id,
                eventType
            }
        });

        var vapid = new VapidDetails(subject, publicKey, privateKey);
        using var client = new WebPushClient();
        var sent = 0;

        foreach (var item in subscriptions)
        {
            try
            {
                var subscription = new WebPush.PushSubscription(item.Endpoint, item.P256dh, item.Auth);
                await client.SendNotificationAsync(subscription, payload, vapid, cancellationToken);
                sent++;
            }
            catch (WebPushException ex) when ((int?)ex.StatusCode is 404 or 410)
            {
                item.Enabled = false;
                item.UpdatedAtUtc = DateTime.UtcNow;
                _logger.LogInformation(ex, "Disabled expired push subscription {SubscriptionId}", item.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Mission push failed for subscription {SubscriptionId}", item.Id);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return sent;
    }

    private async Task EnsurePushSchemaAsync(CancellationToken cancellationToken)
    {
        await _context.Database.ExecuteSqlRawAsync("""
IF OBJECT_ID(N'[dbo].[UserPushSubscriptions]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[UserPushSubscriptions](
        [ID] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_UserPushSubscriptions] PRIMARY KEY,
        [UserId] INT NOT NULL,
        [Endpoint] NVARCHAR(2048) NOT NULL,
        [P256dh] NVARCHAR(512) NOT NULL,
        [Auth] NVARCHAR(256) NOT NULL,
        [UserAgent] NVARCHAR(1024) NULL,
        [CreatedAtUtc] DATETIME2 NOT NULL CONSTRAINT [DF_UserPushSubscriptions_CreatedAtUtc] DEFAULT(SYSUTCDATETIME()),
        [UpdatedAtUtc] DATETIME2 NOT NULL CONSTRAINT [DF_UserPushSubscriptions_UpdatedAtUtc] DEFAULT(SYSUTCDATETIME()),
        [LastSeenAtUtc] DATETIME2 NOT NULL CONSTRAINT [DF_UserPushSubscriptions_LastSeenAtUtc] DEFAULT(SYSUTCDATETIME()),
        [Enabled] BIT NOT NULL CONSTRAINT [DF_UserPushSubscriptions_Enabled] DEFAULT(1),
        CONSTRAINT [FK_UserPushSubscriptions_Users_v2_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users_v2]([Id]) ON DELETE CASCADE
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_UserPushSubscriptions_UserId' AND object_id = OBJECT_ID(N'[dbo].[UserPushSubscriptions]'))
    CREATE INDEX [IX_UserPushSubscriptions_UserId] ON [dbo].[UserPushSubscriptions]([UserId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_UserPushSubscriptions_Endpoint' AND object_id = OBJECT_ID(N'[dbo].[UserPushSubscriptions]'))
    CREATE UNIQUE INDEX [IX_UserPushSubscriptions_Endpoint] ON [dbo].[UserPushSubscriptions]([Endpoint]);
""", cancellationToken);
    }
}
