using Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Services;

public interface IRootOwnerGuard
{
    Task<bool> IsRootOwnerAsync(int userId, CancellationToken cancellationToken = default);

    Task<bool> IsRootOwnerAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns 403 ROOT_OWNER_PROTECTED when the target is a root owner; otherwise null.
    /// </summary>
    Task<IActionResult?> BlockTargetMutationAsync(
        int targetUserId,
        string actionAttempted,
        HttpContext httpContext,
        CancellationToken cancellationToken = default);

    IActionResult ForbiddenRootOwnerOnly();

    IActionResult ForbiddenRootOwnerProtected(string actionAttempted, int? targetUserId = null);
}
