using Contracts.Device;

namespace WebApi.Extensions.Mapping;

public static class DeviceMappingExtensions
{
    public static Device ToContract(this Entities.Device device)
    {
        return new Device
        {
            Id = device.Id.ToString(),
            Title = device.Name,
            Amount = device.Total ?? 0
        };
    }

    public static Entities.Device ToEntity(this CreateDeviceRequest request)
    {
        return new Entities.Device
        {
            Name = request.Title,
            Total = request.Amount
        };
    }

    public static void UpdateFromRequest(this Entities.Device device, UpdateDeviceRequest request)
    {
        device.Name = request.Title;
        device.Total = request.Amount;
    }
}
