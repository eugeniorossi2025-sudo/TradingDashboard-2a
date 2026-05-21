using Contracts.Device;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Services;

namespace WebApi.Controllers;

/// <summary>
/// Controller for managing devices.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DeviceController : ControllerBase
{
    private readonly IDeviceService _deviceService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceController"/> class.
    /// </summary>
    /// <param name="deviceService">The device service.</param>
    public DeviceController(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    /// <summary>
    /// Creates a new device.
    /// </summary>
    /// <param name="request">The create device request.</param>
    /// <returns>The created device.</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDeviceRequest request)
    {
        var device = await _deviceService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = device.Id }, device);
    }

    /// <summary>
    /// Gets a device by its identifier.
    /// </summary>
    /// <param name="id">The device identifier.</param>
    /// <returns>The device if found; otherwise, a not found result.</returns>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var device = await _deviceService.GetByIdAsync(id);
        if (device == null)
            return NotFound();
        return Ok(device);
    }

    /// <summary>
    /// Gets all devices.
    /// </summary>
    /// <returns>A collection of all devices.</returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var devices = await _deviceService.GetAllAsync();
        return Ok(devices);
    }

    /// <summary>
    /// Updates an existing device.
    /// </summary>
    /// <param name="id">The device identifier.</param>
    /// <param name="request">The update device request.</param>
    /// <returns>A no content result if successful; otherwise, a not found result.</returns>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDeviceRequest request)
    {
        var success = await _deviceService.UpdateAsync(id, request);
        if (!success)
            return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Deletes a device.
    /// </summary>
    /// <param name="id">The device identifier.</param>
    /// <returns>A no content result if successful; otherwise, a not found result.</returns>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _deviceService.DeleteAsync(id);
        if (!success)
            return NotFound();
        return NoContent();
    }
}