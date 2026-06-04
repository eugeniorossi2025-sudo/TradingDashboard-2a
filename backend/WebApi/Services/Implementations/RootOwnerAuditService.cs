using Entities;
using Microsoft.EntityFrameworkCore;
using WebApi.Constants;
using WebApi.Data;

namespace WebApi.Services.Implementations;

public class RootOwnerAuditService : IRootOwnerAuditService
{
    private readonly AppDbContext _context;
    private readonly IRootOwnerSchemaService _schema;

    public RootOwnerAuditService(AppDbContext context, IRootOwnerSchemaService schema)
    {
        _context = context;
        _schema = schema;
    }

    public async Task WriteAsync(
        int? actorUserId,
        string? actorUsername,
        string action,
        string outcome,
        string? reason,
        string? detailsJson,
        HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        await _schema.EnsureSchemaAsync(cancellationToken);

        _context.RootOwnerAuditEvents.Add(new RootOwnerAuditEvent
        {
            ActorUserId = actorUserId,
            ActorUsername = actorUsername,
            Action = action,
            OccurredAtUtc = DateTime.UtcNow,
            IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Truncate(httpContext.Request.Headers.UserAgent.ToString(), 1024),
            Outcome = outcome,
            Reason = reason,
            DetailsJson = detailsJson
        });

        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task WriteBlockedMutationAsync(
        int? actorUserId,
        string? actorUsername,
        string actionAttempted,
        int targetUserId,
        HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        return WriteAsync(
            actorUserId,
            actorUsername,
            actionAttempted,
            "BLOCKED",
            AuthConstants.RootOwnerErrorCodes.RootOwnerProtected,
            $"{{\"targetUserId\":{targetUserId}}}",
            httpContext,
            cancellationToken);
    }

    public async Task<IReadOnlyList<RootOwnerAuditDto>> GetRecentAsync(int limit, CancellationToken cancellationToken = default)
    {
        await _schema.EnsureSchemaAsync(cancellationToken);
        limit = Math.Clamp(limit, 1, 100);

        return await _context.RootOwnerAuditEvents.AsNoTracking()
            .OrderByDescending(e => e.OccurredAtUtc)
            .Take(limit)
            .Select(e => new RootOwnerAuditDto
            {
                Id = e.Id,
                ActorUserId = e.ActorUserId,
                ActorUsername = e.ActorUsername,
                Action = e.Action,
                OccurredAtUtc = e.OccurredAtUtc,
                IpAddress = e.IpAddress,
                Outcome = e.Outcome,
                Reason = e.Reason
            })
            .ToListAsync(cancellationToken);
    }

    private static string? Truncate(string? value, int max)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= max ? value : value[..max];
    }
}
