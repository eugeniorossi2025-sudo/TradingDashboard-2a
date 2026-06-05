namespace WebApi.Services;

public interface IControlRoomCommandOverrideService
{
    Task EnsureSchemaAsync(CancellationToken cancellationToken = default);

    Task<ControlRoomCommandOverrideResult> SetContinueAsync(string pc, int? userId, CancellationToken cancellationToken = default);

    Task<ControlRoomCommandOverrideResult> SetResetMartingaleAsync(string pc, int? userId, CancellationToken cancellationToken = default);
}

public sealed class ControlRoomCommandOverrideResult
{
    public string Pc { get; set; } = string.Empty;
    public int ActionCode { get; set; }
    public string CommandType { get; set; } = string.Empty;
}
