using Contracts.Device;

namespace WebApi.Extensions.Mapping;

/// <summary>
/// Provides extension methods to map between Device entities and DTOs.
/// </summary>
public static class DeviceMappingExtensions
{
    /// <summary>
    /// Maps a Device entity to a Device DTO.
    /// </summary>
    /// <param name="device">The device entity.</param>
    /// <returns>A Device DTO.</returns>
    public static Device ToContract(this Entities.Device device)
    {
        return new Device
        {
            Id = device.Id,
            Title = device.Title,
            Amount = device.Amount
        };
    }

    /// <summary>
    /// Maps a CreateDeviceRequest DTO to a Device entity.
    /// </summary>
    /// <param name="request">The create device request.</param>
    /// <returns>A new Device entity.</returns>
    public static Entities.Device ToEntity(this CreateDeviceRequest request)
    {
        return new Entities.Device
        {
            Id = request.Id,      // ✅ AGGIUNGI QUESTA RIGA
            Title = request.Title,
            Amount = request.Amount
        };
    }

    /// <summary>
    /// Updates a Device entity from an UpdateDeviceRequest DTO.
    /// </summary>
    /// <param name="device">The device entity to update.</param>
    /// <param name="request">The update device request.</param>
    public static void UpdateFromRequest(this Entities.Device device, UpdateDeviceRequest request)
    {
        device.Title = request.Title;
        device.Amount = request.Amount;
    }
}