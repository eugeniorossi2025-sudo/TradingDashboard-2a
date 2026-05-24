namespace WebApi.Services;

public interface IUserAccessTracker
{
    Task TrackAsync(int? userId, string? username, string eventType, string? page, HttpContext? httpContext);
}
