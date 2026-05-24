using Entities;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;

namespace WebApi.Services.Implementations;

public class UserAccessTracker : IUserAccessTracker
{
    private readonly AppDbContext _context;

    public UserAccessTracker(AppDbContext context)
    {
        _context = context;
    }

    public async Task TrackAsync(int? userId, string? username, string eventType, string? page, HttpContext? httpContext)
    {
        await EnsureAccessEventsSchemaAsync();

        var normalizedEvent = string.IsNullOrWhiteSpace(eventType) ? "PAGE_VIEW" : eventType.Trim().ToUpperInvariant();
        var ip = httpContext?.Connection.RemoteIpAddress?.ToString();
        var userAgent = httpContext?.Request.Headers.UserAgent.ToString();

        _context.UserAccessEvents.Add(new UserAccessEvent
        {
            UserId = userId,
            Username = username,
            EventType = normalizedEvent.Length > 32 ? normalizedEvent[..32] : normalizedEvent,
            IpAddress = ip,
            Page = page,
            UserAgent = userAgent,
            OccurredAtUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }

    private async Task EnsureAccessEventsSchemaAsync()
    {
        const string sql = """
IF OBJECT_ID(N'[dbo].[UserAccessEvents]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[UserAccessEvents](
        [ID] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_UserAccessEvents] PRIMARY KEY,
        [UserId] INT NULL,
        [Username] NVARCHAR(256) NULL,
        [EventType] NVARCHAR(32) NOT NULL,
        [IpAddress] NVARCHAR(128) NULL,
        [Page] NVARCHAR(512) NULL,
        [UserAgent] NVARCHAR(1024) NULL,
        [OccurredAtUtc] DATETIME2 NOT NULL CONSTRAINT [DF_UserAccessEvents_OccurredAtUtc] DEFAULT(SYSUTCDATETIME())
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_UserAccessEvents_UserId' AND object_id = OBJECT_ID(N'[dbo].[UserAccessEvents]'))
    CREATE INDEX [IX_UserAccessEvents_UserId] ON [dbo].[UserAccessEvents]([UserId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_UserAccessEvents_OccurredAtUtc' AND object_id = OBJECT_ID(N'[dbo].[UserAccessEvents]'))
    CREATE INDEX [IX_UserAccessEvents_OccurredAtUtc] ON [dbo].[UserAccessEvents]([OccurredAtUtc]);
""";
        await _context.Database.ExecuteSqlRawAsync(sql);
    }
}
