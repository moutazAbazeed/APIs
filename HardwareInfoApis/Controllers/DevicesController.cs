using HardwareInfoApis.Api.Models.Api.Responses;
using HardwareInfoApis.Api.Services.Interfaces;
using HardwareInfoApis.Models.Api.Requests;
using HardwareInfoApis.Models.Api.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace HardwareInfoApis.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceService _deviceService;
        private readonly ILogger<DevicesController> _logger;

        public DevicesController(
            IDeviceService deviceService,
            ILogger<DevicesController> logger)
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                return BadRequest(ApiResponse<object>.Error("Request body is required",Models.Api.ApiErrorCode.InvalidRequest));

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
                return BadRequest(ApiResponse<object>.Error("Request body is required",Models.Api.ApiErrorCode.InvalidRequest));

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
            // Implementation for admin device lookup
            // ...
            return NotImplementedException;
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