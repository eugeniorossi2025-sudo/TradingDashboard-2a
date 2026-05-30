namespace WebApi.Services;

public interface IL7AlertWatchService
{
    Task CheckAsync(CancellationToken cancellationToken = default);
}
