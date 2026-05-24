using System.Security.Claims;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Constants;
using WebApi.Data;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = AuthConstants.Policies.RequireAdmin)]
[Produces("application/json")]
public class AdminUsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IUserAccessTracker _accessTracker;
    private readonly IEmailSender _emailSender;

    public AdminUsersController(
        AppDbContext context,
        UserManager<User> userManager,
        IUserAccessTracker accessTracker,
        IEmailSender emailSender)
    {
        _context = context;
        _userManager = userManager;
        _accessTracker = accessTracker;
        _emailSender = emailSender;
    }

    [HttpGet("users/overview")]
    public async Task<IActionResult> GetOverview()
    {
        await EnsureAdminUserSchemaAsync();

        var users = await _context.Users.AsNoTracking().OrderBy(user => user.UserName).ToListAsync();
        var userIds = users.Select(user => user.Id).ToArray();
        var lastEvents = await _context.UserAccessEvents
            .AsNoTracking()
            .Where(access => access.UserId.HasValue && userIds.Contains(access.UserId.Value))
            .GroupBy(access => access.UserId!.Value)
            .Select(group => group.OrderByDescending(access => access.OccurredAtUtc).First())
            .ToDictionaryAsync(access => access.UserId!.Value);

        var rolesByUser = new Dictionary<int, IList<string>>();
        foreach (var user in users)
        {
            rolesByUser[user.Id] = await _userManager.GetRolesAsync(user);
        }

        var rows = users.Select(user =>
        {
            lastEvents.TryGetValue(user.Id, out var lastEvent);
            var roles = rolesByUser[user.Id].ToArray();
            var role = roles.FirstOrDefault() ?? (user.Admin ? AuthConstants.Roles.Admin : AuthConstants.Roles.User);
            var accountType = GetAccountType(user, roles);
            return new AdminUserOverviewRow
            {
                UserId = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email,
                Role = role,
                Roles = roles,
                AccountType = accountType,
                Status = IsRecentlyActive(lastEvent?.OccurredAtUtc) ? "Online" : "Offline",
                LastLoginUtc = user.LastLogin,
                LastIp = lastEvent?.IpAddress,
                LastPage = lastEvent?.Page,
                LastEvent = lastEvent == null ? null : $"{lastEvent.EventType} · {lastEvent.OccurredAtUtc:dd/MM/yyyy, HH:mm}",
                Enabled = !user.LockoutEnd.HasValue || user.LockoutEnd <= DateTimeOffset.UtcNow
            };
        }).ToList();

        var response = new AdminUsersOverviewResponse
        {
            Operative = rows.Where(row => row.Role != AuthConstants.Roles.Admin && row.AccountType != "Bot").ToList(),
            Bots = rows.Where(row => row.AccountType == "Bot").ToList(),
            Admins = rows.Where(row => row.Role == AuthConstants.Roles.Admin).ToList()
        };

        return Ok(ApiResponse<AdminUsersOverviewResponse>.SuccessResponse(response));
    }

    [HttpGet("user-notification-settings")]
    public async Task<IActionResult> GetNotificationSettings()
    {
        await EnsureAdminUserSchemaAsync();

        var users = await _context.Users.AsNoTracking().OrderBy(user => user.UserName).ToListAsync();
        var settings = await _context.UserNotificationSettings.AsNoTracking().ToDictionaryAsync(setting => setting.UserId);

        var rows = users.Select(user =>
        {
            settings.TryGetValue(user.Id, out var setting);
            return new UserNotificationSettingDto
            {
                UserId = user.Id,
                Username = user.UserName ?? string.Empty,
                LoginEmail = user.Email,
                NotificationEmail = setting?.NotificationEmail ?? user.Email,
                Enabled = setting?.Enabled ?? true,
                Mission = setting?.Mission ?? true,
                System = setting?.System ?? true,
                Errors = setting?.Errors ?? true
            };
        }).ToList();

        return Ok(ApiResponse<List<UserNotificationSettingDto>>.SuccessResponse(rows));
    }

    [HttpPut("user-notification-settings/{userId:int}")]
    public async Task<IActionResult> UpdateNotificationSetting(int userId, [FromBody] UpdateUserNotificationSettingRequest request)
    {
        await EnsureAdminUserSchemaAsync();

        var userExists = await _context.Users.AnyAsync(user => user.Id == userId);
        if (!userExists)
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

        var setting = await _context.UserNotificationSettings.FirstOrDefaultAsync(row => row.UserId == userId);
        if (setting == null)
        {
            setting = new UserNotificationSetting
            {
                UserId = userId,
                CreatedAtUtc = DateTime.UtcNow
            };
            _context.UserNotificationSettings.Add(setting);
        }

        setting.NotificationEmail = request.NotificationEmail;
        setting.Enabled = request.Enabled;
        setting.Mission = request.Mission;
        setting.System = request.System;
        setting.Errors = request.Errors;
        setting.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new object(), "Notification settings saved"));
    }

    [HttpPost("user-notification-settings/{userId:int}/test")]
    public async Task<IActionResult> SendTestEmail(int userId)
    {
        await EnsureAdminUserSchemaAsync();

        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(row => row.Id == userId);
        if (user == null)
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

        var setting = await _context.UserNotificationSettings.AsNoTracking().FirstOrDefaultAsync(row => row.UserId == userId);
        var to = setting?.NotificationEmail ?? user.Email;
        if (string.IsNullOrWhiteSpace(to))
            return BadRequest(ApiResponse<object>.ErrorResponse("Email notifiche mancante"));

        try
        {
            await _emailSender.SendAsync(
                to,
                "DASH2A - Test notifiche",
                $"Test notifiche DASH2A per {user.UserName}. Ora UTC: {DateTime.UtcNow:O}");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(new object(), "Test email sent"));
    }

    [HttpGet("users/{userId:int}/access-report")]
    public async Task<IActionResult> GetAccessReport(int userId, [FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc, [FromQuery] int limit = 250)
    {
        await EnsureAdminUserSchemaAsync();

        var from = fromUtc ?? DateTime.UtcNow.AddDays(-30);
        var toExclusive = (toUtc ?? DateTime.UtcNow).AddDays(1);
        limit = Math.Clamp(limit, 1, 1000);

        var events = await _context.UserAccessEvents
            .AsNoTracking()
            .Where(access => access.UserId == userId && access.OccurredAtUtc >= from && access.OccurredAtUtc < toExclusive)
            .OrderByDescending(access => access.OccurredAtUtc)
            .Take(limit)
            .Select(access => new UserAccessEventDto
            {
                Id = access.Id,
                UserId = access.UserId,
                Username = access.Username,
                EventType = access.EventType,
                IpAddress = access.IpAddress,
                Page = access.Page,
                UserAgent = access.UserAgent,
                OccurredAtUtc = access.OccurredAtUtc
            })
            .ToListAsync();

        return Ok(ApiResponse<List<UserAccessEventDto>>.SuccessResponse(events));
    }

    [HttpPost("access-events")]
    public async Task<IActionResult> TrackAccessEvent([FromBody] TrackAccessEventRequest request)
    {
        await EnsureAdminUserSchemaAsync();

        var userIdValue = User.FindFirst(AuthConstants.Claims.UserId)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(userIdValue, out var userId);
        var username = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.UniqueName)?.Value ?? User.Identity?.Name;

        await _accessTracker.TrackAsync(userId == 0 ? null : userId, username, request.EventType, request.Page, HttpContext);
        return Ok(ApiResponse<object>.SuccessResponse(new object(), "Access event tracked"));
    }

    private static bool IsRecentlyActive(DateTime? occurredAtUtc)
    {
        return occurredAtUtc.HasValue && occurredAtUtc.Value >= DateTime.UtcNow.AddMinutes(-5);
    }

    private static string GetAccountType(User user, IReadOnlyCollection<string> roles)
    {
        if (roles.Any(role => role.Equals("Bot", StringComparison.OrdinalIgnoreCase) || role.Equals("BotOperator", StringComparison.OrdinalIgnoreCase)))
            return "Bot";

        if ((user.UserName ?? string.Empty).Contains("bot", StringComparison.OrdinalIgnoreCase))
            return "Bot";

        return string.IsNullOrWhiteSpace(user.Email) ? "Legacy" : "Human";
    }

    private async Task EnsureAdminUserSchemaAsync()
    {
        const string sql = """
IF OBJECT_ID(N'[dbo].[UserNotificationSettings]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[UserNotificationSettings](
        [ID] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_UserNotificationSettings] PRIMARY KEY,
        [UserId] INT NOT NULL,
        [NotificationEmail] NVARCHAR(256) NULL,
        [Enabled] BIT NOT NULL CONSTRAINT [DF_UserNotificationSettings_Enabled] DEFAULT(1),
        [Mission] BIT NOT NULL CONSTRAINT [DF_UserNotificationSettings_Mission] DEFAULT(1),
        [System] BIT NOT NULL CONSTRAINT [DF_UserNotificationSettings_System] DEFAULT(1),
        [Errors] BIT NOT NULL CONSTRAINT [DF_UserNotificationSettings_Errors] DEFAULT(1),
        [CreatedAtUtc] DATETIME2 NOT NULL CONSTRAINT [DF_UserNotificationSettings_CreatedAtUtc] DEFAULT(SYSUTCDATETIME()),
        [UpdatedAtUtc] DATETIME2 NOT NULL CONSTRAINT [DF_UserNotificationSettings_UpdatedAtUtc] DEFAULT(SYSUTCDATETIME())
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_UserNotificationSettings_UserId' AND object_id = OBJECT_ID(N'[dbo].[UserNotificationSettings]'))
    CREATE UNIQUE INDEX [IX_UserNotificationSettings_UserId] ON [dbo].[UserNotificationSettings]([UserId]);
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

public class AdminUsersOverviewResponse
{
    public List<AdminUserOverviewRow> Operative { get; set; } = new();
    public List<AdminUserOverviewRow> Bots { get; set; } = new();
    public List<AdminUserOverviewRow> Admins { get; set; } = new();
}

public class AdminUserOverviewRow
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Role { get; set; } = string.Empty;
    public string[] Roles { get; set; } = [];
    public string AccountType { get; set; } = "Human";
    public string Status { get; set; } = "Offline";
    public DateTime? LastLoginUtc { get; set; }
    public string? LastIp { get; set; }
    public string? LastPage { get; set; }
    public string? LastEvent { get; set; }
    public bool Enabled { get; set; }
}

public class UserNotificationSettingDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? LoginEmail { get; set; }
    public string? NotificationEmail { get; set; }
    public bool Enabled { get; set; }
    public bool Mission { get; set; }
    public bool System { get; set; }
    public bool Errors { get; set; }
}

public class UpdateUserNotificationSettingRequest
{
    public string? NotificationEmail { get; set; }
    public bool Enabled { get; set; }
    public bool Mission { get; set; }
    public bool System { get; set; }
    public bool Errors { get; set; }
}

public class TrackAccessEventRequest
{
    public string EventType { get; set; } = "PAGE_VIEW";
    public string? Page { get; set; }
}

public class UserAccessEventDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? Page { get; set; }
    public string? UserAgent { get; set; }
    public DateTime OccurredAtUtc { get; set; }
}
