namespace WebApi.Services;

public interface IRootOwnerAuditService
{
    Task WriteAsync(
        int? actorUserId,
        string? actorUsername,
        string action,
        string outcome,
        string? reason,
        string? detailsJson,
        HttpContext httpContext,
        CancellationToken cancellationToken = default);

    Task WriteBlockedMutationAsync(
        int? actorUserId,
        string? actorUsername,
        string actionAttempted,
        int targetUserId,
        HttpContext httpContext,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RootOwnerAuditDto>> GetRecentAsync(int limit, CancellationToken cancellationToken = default);
}

public sealed class RootOwnerAuditDto
{
    public int Id { get; set; }
    public int? ActorUserId { get; set; }
    public string? ActorUsername { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
    public string? IpAddress { get; set; }
    public string Outcome { get; set; } = string.Empty;
    public string? Reason { get; set; }
}
