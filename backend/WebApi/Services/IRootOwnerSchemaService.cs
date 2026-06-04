namespace WebApi.Services;

public interface IRootOwnerSchemaService
{
    Task EnsureSchemaAsync(CancellationToken cancellationToken = default);
}
