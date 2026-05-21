using Contracts.Device;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Extensions.Mapping;

namespace WebApi.Services.Implementations;

/// <summary>
/// Service for managing device operations.
/// </summary>
public class DeviceService : IDeviceService
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceService"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public DeviceService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new device asynchronously.
    /// </summary>
    /// <param name="request">The create device request.</param>
    /// <returns>The created device.</returns>
    public async Task<Device> CreateAsync(CreateDeviceRequest request)
    {
        var device = request.ToEntity();
        _context.Devices.Add(device);
        await _context.SaveChangesAsync();
        return device.ToContract();
    }

    /// <summary>
    /// Gets a device by its identifier asynchronously.
    /// </summary>
    /// <param name="id">The device identifier.</param>
    /// <returns>The device if found; otherwise, null.</returns>
    public async Task<Device?> GetByIdAsync(int id)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device == null)
            return null;

        return device.ToContract();
    }

    /// <summary>
    /// Gets all devices asynchronously.
    /// </summary>
    /// <returns>A collection of all devices.</returns>
    public async Task<IEnumerable<Device>> GetAllAsync()
    {
        return await _context.Devices
            .Select(d => d.ToContract())
            .ToListAsync();
    }

    /// <summary>
    /// Updates an existing device asynchronously.
    /// </summary>
    /// <param name="id">The device identifier.</param>
    /// <param name="request">The update device request.</param>
    /// <returns>True if the device was updated; otherwise, false.</returns>
    public async Task<bool> UpdateAsync(int id, UpdateDeviceRequest request)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device == null)
            return false;

        device.UpdateFromRequest(request);
        _context.Devices.Update(device);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Deletes a device asynchronously.
    /// </summary>
    /// <param name="id">The device identifier.</param>
    /// <returns>True if the device was deleted; otherwise, false.</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device == null)
            return false;

        _context.Devices.Remove(device);
        await _context.SaveChangesAsync();
        return true;
    }
}