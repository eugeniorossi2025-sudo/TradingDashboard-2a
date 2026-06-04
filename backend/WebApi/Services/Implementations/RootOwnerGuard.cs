using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Constants;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Services.Implementations;

public class RootOwnerGuard : IRootOwnerGuard
{
    private readonly AppDbContext _context;
    private readonly IRootOwnerAuditService _audit;

    public RootOwnerGuard(AppDbContext context, IRootOwnerAuditService audit)
    {
        _context = context;
        _audit = audit;
    }

    public Task<bool> IsRootOwnerAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _context.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.IsRootOwner)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> IsRootOwnerAsync(User user, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(user.IsRootOwner);
    }

    public async Task<IActionResult?> BlockTargetMutationAsync(
        int targetUserId,
        string actionAttempted,
        HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        var isProtected = await IsRootOwnerAsync(targetUserId, cancellationToken);
        if (!isProtected)
            return null;

        var (actorId, actorName) = ReadActor(httpContext);
        await _audit.WriteBlockedMutationAsync(actorId, actorName, actionAttempted, targetUserId, httpContext, cancellationToken);
        return ForbiddenRootOwnerProtected(actionAttempted, targetUserId);
    }

    public IActionResult ForbiddenRootOwnerOnly()
    {
        return new ObjectResult(ApiResponse<object>.ErrorResponse(
            "Root owner access only.",
            code: AuthConstants.RootOwnerErrorCodes.RootOwnerOnly))
        {
            StatusCode = StatusCodes.Status403Forbidden
        };
    }

    public IActionResult ForbiddenRootOwnerProtected(string actionAttempted, int? targetUserId = null)
    {
        var errors = targetUserId.HasValue
            ? new List<string> { $"action={actionAttempted}", $"targetUserId={targetUserId.Value}" }
            : new List<string> { $"action={actionAttempted}" };

        return new ObjectResult(ApiResponse<object>.ErrorResponse(
            "Root owner account is protected.",
            errors,
            AuthConstants.RootOwnerErrorCodes.RootOwnerProtected))
        {
            StatusCode = StatusCodes.Status403Forbidden
        };
    }

    private static (int? actorId, string? actorName) ReadActor(HttpContext httpContext)
    {
        var user = httpContext.User;
        var userIdValue = user.FindFirst(AuthConstants.Claims.UserId)?.Value
            ?? user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(userIdValue, out var actorId);
        var actorName = user.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.UniqueName)?.Value
            ?? user.Identity?.Name;
        return actorId == 0 ? (null, actorName) : (actorId, actorName);
    }
}
