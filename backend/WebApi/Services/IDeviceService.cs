using Contracts.Device;

namespace WebApi.Services;

/// <summary>
/// Interface for managing device CRUD operations.
/// </summary>
public interface IDeviceService
{
    /// <summary>
    /// Creates a new device asynchronously.
    /// </summary>
    /// <param name="request">The create device request.</param>
    /// <returns>The created device.</returns>
    Task<Device> CreateAsync(CreateDeviceRequest request);

    /// <summary>
    /// Gets a device by its identifier asynchronously.
    /// </summary>
    /// <param name="id">The device identifier.</param>
    /// <returns>The device if found; otherwise, null.</returns>
    Task<Device?> GetByIdAsync(int id);

    /// <summary>
    /// Gets all devices asynchronously.
    /// </summary>
    /// <returns>A collection of all devices.</returns>
    Task<IEnumerable<Device>> GetAllAsync();

    /// <summary>
    /// Updates an existing device asynchronously.
    /// </summary>
    /// <param name="id">The device identifier.</param>
    /// <param name="request">The update device request.</param>
    /// <returns>True if the device was updated; otherwise, false.</returns>
    Task<bool> UpdateAsync(int id, UpdateDeviceRequest request);

    /// <summary>
    /// Deletes a device asynchronously.
    /// </summary>
    /// <param name="id">The device identifier.</param>
    /// <returns>True if the device was deleted; otherwise, false.</returns>
    Task<bool> DeleteAsync(int id);
}