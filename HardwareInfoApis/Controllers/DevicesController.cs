using HardwareInfoApis.Api.Models.Api.Responses;
using HardwareInfoApis.Api.Services.Interfaces;
using HardwareInfoApis.Models.Api.Requests;
using HardwareInfoApis.Models.Api.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using HardwareInfoApis.Api.Data;
using HardwareInfoApis.Api.Models.Entities;

namespace HardwareInfoApis.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceService _deviceService;
        private readonly ILogger<DevicesController> _logger;
        private readonly ApplicationDbContext _db;

        public DevicesController(
            IDeviceService deviceService,
            ILogger<DevicesController> logger,
            ApplicationDbContext db)
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        /// <summary>
        /// Check if a device is registered
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/devices/check
        ///     {
        ///         "deviceFingerprint": "abc123...",
        ///         "appVersion": "1.0.0"
        ///     }
        /// </remarks>
        [HttpPost("check")]
        [AllowAnonymous]  // Or [Authorize] if requiring auth
        [ProducesResponseType(typeof(ApiResponse<CheckDeviceResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<CheckDeviceResponse>>> CheckDevice(
            [FromBody] CheckDeviceRequest request,
            CancellationToken ct = default)
        {
            if (request == null)
                return BadRequest(ApiResponse<object>.Error("Request body is required", Models.Api.ApiErrorCode.InvalidRequest));

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Error("Invalid request", Models.Api.ApiErrorCode.InvalidRequest));

            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            _logger.LogInformation("CheckDevice request from {ClientIp} for fingerprint: {Fingerprint}",
                clientIp, request.DeviceFingerprint);

            var response = await _deviceService.CheckDeviceAsync(request, clientIp, ct);

            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Register a new device
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]  // Or [Authorize] if requiring auth
        [ProducesResponseType(typeof(ApiResponse<RegisterDeviceResponse>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 409)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<RegisterDeviceResponse>>> RegisterDevice(
            [FromBody] RegisterDeviceRequest request,
            CancellationToken ct = default)
        {
            if (request == null)
                return BadRequest(ApiResponse<object>.Error("Request body is required", Models.Api.ApiErrorCode.InvalidRequest));

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Error("Invalid request", Models.Api.ApiErrorCode.InvalidRequest));

            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            _logger.LogInformation("RegisterDevice request from {ClientIp} for fingerprint: {Fingerprint}",
                clientIp, request.DeviceFingerprint);

            var response = await _deviceService.RegisterDeviceAsync(request, clientIp, ct);

            if (!response.Success)
                return BadRequest(response);

            // Return 201 Created for new registrations
            return CreatedAtAction(nameof(GetDevice), new { id = response.Data?.DeviceId }, response);
        }

        /// <summary>
        /// Get device details by ID (admin only)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]  // Require admin role
        [ProducesResponseType(typeof(ApiResponse<DeviceDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ApiResponse<DeviceDto>>> GetDevice(
            [FromRoute] string id,
            CancellationToken ct = default)
        {
            try
            {
                Device? device = null;

                // Try parse as numeric database id first
                if (int.TryParse(id, out var numericId))
                {
                    device = await _db.Devices
                        .Include(d => d.License)
                        .FirstOrDefaultAsync(d => d.Id == numericId, ct);
                }
                else
                {
                    // Fallback: treat id as device fingerprint
                    device = await _db.Devices
                        .Include(d => d.License)
                        .FirstOrDefaultAsync(d => d.DeviceFingerprint == id, ct);
                }

                if (device == null)
                {
                    return NotFound();
                }

                var dto = new DeviceDto
                {
                    Id = device.Id,
                    DeviceFingerprint = device.DeviceFingerprint,
                    DeviceName = device.DeviceName,
                    RegisteredAt = device.RegisteredAt,
                    LastSeenAt = device.LastSeenAt,
                    LicenseStatus = device.License == null ? "None" :
                                    device.License.IsRevoked ? "Revoked" :
                                    device.License.IsActive ? "Active" : "Inactive"
                };

                return Ok(ApiResponse<DeviceDto>.SuccessResponse(dto, "Device found"));
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("GetDevice cancelled for id: {Id}", id);
                return StatusCode(499);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting device: {Id}", id);
                return StatusCode(500, ApiResponse<DeviceDto>.Error("Internal server error", Models.Api.ApiErrorCode.ServerError));
            }
        }

        /// <summary>
        /// Update device last seen timestamp (heartbeat)
        /// </summary>
        [HttpPost("{id}/heartbeat")]
        [AllowAnonymous]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Heartbeat(
            [FromRoute] int id,
            [FromBody] HeartbeatRequest request,
            CancellationToken ct = default)
        {
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            var success = await _deviceService.UpdateDeviceLastSeenAsync(
                id, clientIp, request?.AppVersion, ct);

            return success ? NoContent() : NotFound();
        }
    }

    // Helper DTOs
    public class HeartbeatRequest
    {
        public string AppVersion { get; set; }
    }

    public class DeviceDto
    {
        public int Id { get; set; }
        public string DeviceFingerprint { get; set; }
        public string DeviceName { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime? LastSeenAt { get; set; }
        public string LicenseStatus { get; set; }
    }
}