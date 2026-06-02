namespace WebApi.Services;

public interface IPushNotificationService
{
    PushConfigurationState GetConfigurationState();
    Task SaveSubscriptionAsync(int userId, PushSubscriptionRequest request, string? userAgent, CancellationToken cancellationToken = default);
    Task<int> SendMissionNotificationAsync(int sessionId, string eventType, CancellationToken cancellationToken = default);
    Task<int> SendAdminBotLevelAlertAsync(string computer, string? tavolo, int level, decimal margine, CancellationToken cancellationToken = default);
    Task<int> SendTestNotificationToUserAsync(int userId, string? deepLinkUrl, CancellationToken cancellationToken = default);
}

public sealed class PushConfigurationState
{
    public bool Enabled { get; set; }
    public string? PublicKey { get; set; }
}

public sealed class PushSubscriptionRequest
{
    public string Endpoint { get; set; } = string.Empty;
    public PushSubscriptionKeys Keys { get; set; } = new();
}

public sealed class PushSubscriptionKeys
{
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
}
