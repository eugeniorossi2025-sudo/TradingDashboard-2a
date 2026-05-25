using WebApi.Models;

namespace WebApi.Services;

public interface IPcCurrentStatusMirrorService
{
    Task MirrorAsync(MirrorPcStatusRequest request, CancellationToken cancellationToken = default);

    Task<MirrorPcStatusRequest?> GetPcStatusAsync(string computer, CancellationToken cancellationToken = default);
}
