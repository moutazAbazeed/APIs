using HardwareInfoApis.Api.Data;
using HardwareInfoApis.Api.Models.Api;
using HardwareInfoApis.Api.Models.Api.Responses;
using HardwareInfoApis.Api.Models.Entities;
using HardwareInfoApis.Api.Services.Interfaces;
using HardwareInfoApis.Models.Api;
using HardwareInfoApis.Models.Api.Requests;
using HardwareInfoApis.Models.Api.Responses;
using HardwareInfoApis.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using HardwareInfoApis.Api.Models; // <-- Add this line if ApiSettings is defined here


namespace HardwareInfoApis.Api.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFingerprintService _fingerprintService;
        private readonly ILogger<DeviceService> _logger;
        private readonly ApiSettings _settings;

        public DeviceService(
            ApplicationDbContext context,
            IFingerprintService fingerprintService,
            IOptions<ApiSettings> settings,
            ILogger<DeviceService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _fingerprintService = fingerprintService ?? throw new ArgumentNullException(nameof(fingerprintService));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponse<CheckDeviceResponse>> CheckDeviceAsync(
            CheckDeviceRequest request, string? clientIp, CancellationToken ct = default)
        {
            try
            {
                // Validate fingerprint
                if (string.IsNullOrWhiteSpace(request.DeviceFingerprint))
                {
                    return ApiResponse<CheckDeviceResponse>.Error(
                        "Device fingerprint is required",
                        ApiErrorCode.InvalidFingerprint);
                }

                // Find device
                var device = await _context.Devices
                    .Include(d => d.License)
                    .FirstOrDefaultAsync(d =>
                        d.DeviceFingerprint == request.DeviceFingerprint &&
                        d.IsActive && !d.IsBlocked, ct);

                if (device == null)
                {
                    return new ApiResponse<CheckDeviceResponse>
                    {
                        Success = true,
                        Message = "Device not found",
                        Data = new CheckDeviceResponse { IsRegistered = false }
                    };
                }

                // Update last seen
                device.LastSeenAt = DateTime.UtcNow;
                device.LastIpAddress = clientIp;
                device.LastAppVersion = request.AppVersion;
                await _context.SaveChangesAsync(ct);

                // Build response
                var response = new CheckDeviceResponse
                {
                    IsRegistered = true,
                    RegistrationDate = device.RegisteredAt,
                    LastSeenAt = device.LastSeenAt,
                    DeviceName = device.DeviceName,
                    LicenseStatus = device.License?.IsActive == true ? "Active" :
                                   device.License?.IsRevoked == true ? "Revoked" : "None",
                    RequiresUpdate = device.RequiresUpdate
                };

                return ApiResponse<CheckDeviceResponse>.SuccessResponse(response, "Device found");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("CheckDeviceAsync cancelled for fingerprint: {Fingerprint}", request.DeviceFingerprint);
                return ApiResponse<CheckDeviceResponse>.Error("Request timeout", ApiErrorCode.ServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking device: {Fingerprint}", request.DeviceFingerprint);
                return ApiResponse<CheckDeviceResponse>.Error("Internal server error", ApiErrorCode.ServerError);
            }
        }

        public async Task<ApiResponse<RegisterDeviceResponse>> RegisterDeviceAsync(
            RegisterDeviceRequest request, string? clientIp, CancellationToken ct = default)
        {
            try
            {
                // Validate
                if (string.IsNullOrWhiteSpace(request.DeviceFingerprint))
                    return ApiResponse<RegisterDeviceResponse>.Error(
                        "Device fingerprint is required", ApiErrorCode.InvalidFingerprint);

                // Check if already registered
                var existingDevice = await _context.Devices
                    .FirstOrDefaultAsync(d => d.DeviceFingerprint == request.DeviceFingerprint, ct);

                if (existingDevice != null)
                {
                    if (existingDevice.IsActive && !existingDevice.IsBlocked)
                    {
                        // Already registered - return existing info
                        return ApiResponse<RegisterDeviceResponse>.SuccessResponse(
                            new RegisterDeviceResponse
                            {
                                DeviceId = existingDevice.Id.ToString(),
                                RegistrationDate = existingDevice.RegisteredAt,
                                LicenseKey = existingDevice.License?.LicenseKey
                            },
                            "Device already registered");
                    }
                    else if (existingDevice.IsBlocked)
                    {
                        return ApiResponse<RegisterDeviceResponse>.Error(
                            $"Device blocked: {existingDevice.BlockReason}",
                            ApiErrorCode.DeviceAlreadyRegistered);
                    }
                }

                // Validate license if provided
                License? license = null;
                if (!string.IsNullOrWhiteSpace(request.LicenseKey))
                {
                    license = await _context.Licenses
                        .FirstOrDefaultAsync(l =>
                            l.LicenseKey == request.LicenseKey &&
                            l.IsActive && !l.IsRevoked, ct);

                    if (license == null)
                        return ApiResponse<RegisterDeviceResponse>.Error(
                            "Invalid or expired license key", ApiErrorCode.LicenseInvalid);

                    // Check device limit
                    if (license.MaxDevices > 0 && license.CurrentDeviceCount >= license.MaxDevices)
                        return ApiResponse<RegisterDeviceResponse>.Error(
                            "License device limit reached", ApiErrorCode.LicenseInvalid);
                }

                // Create new device
                var newDevice = new Device
                {
                    DeviceFingerprint = request.DeviceFingerprint,
                    DeviceName = request.HardwareInfo?.OperatingSystem?.MachineName ?? "Unknown Device",
                    HardwareInfoJson = JsonConvert.SerializeObject(request.HardwareInfo),

                    // Indexable hardware fields
                    ProcessorId = request.HardwareInfo?.Processor?.ProcessorId,
                    ProcessorModel = request.HardwareInfo?.Processor?.Model,
                    TotalRam = request.HardwareInfo?.Memory?.TotalPhysicalRam,
                    TotalRamBytes = request.HardwareInfo?.Memory?.TotalPhysicalBytes,
                    BiosSerial = request.HardwareInfo?.Bios?.SerialNumber,
                    DiskSerial = request.HardwareInfo?.Storage?.PrimaryDiskSerial,
                    DiskModel = request.HardwareInfo?.Storage?.PrimaryDiskModel,
                    OsName = request.HardwareInfo?.OperatingSystem?.Name,
                    OsBuild = request.HardwareInfo?.OperatingSystem?.BuildNumber,

                    // Metadata
                    RegisteredAt = DateTime.UtcNow,
                    LastSeenAt = DateTime.UtcNow,
                    LastIpAddress = clientIp,
                    LastAppVersion = request.AppVersion,
                    UserId = null,
                    LicenseId = license?.Id,
                    IsActive = true
                };

                _context.Devices.Add(newDevice);

                // Update license device count
                if (license != null)
                {
                    license.CurrentDeviceCount++;
                    license.ActivatedAt ??= DateTime.UtcNow;
                }

                await _context.SaveChangesAsync(ct);

                _logger.LogInformation("Device registered: {Fingerprint} (ID: {DeviceId})",
                    request.DeviceFingerprint, newDevice.Id);

                return ApiResponse<RegisterDeviceResponse>.SuccessResponse(
                    new RegisterDeviceResponse
                    {
                        DeviceId = newDevice.Id.ToString(),
                        RegistrationDate = newDevice.RegisteredAt,
                        LicenseKey = license?.LicenseKey,
                        LicenseExpiryDate = license?.ExpiresAt
                    },
                    "Device registered successfully");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("RegisterDeviceAsync cancelled for fingerprint: {Fingerprint}", request.DeviceFingerprint);
                return ApiResponse<RegisterDeviceResponse>.Error("Request timeout", ApiErrorCode.ServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering device: {Fingerprint}", request.DeviceFingerprint);
                return ApiResponse<RegisterDeviceResponse>.Error("Internal server error", ApiErrorCode.ServerError);
            }
        }

        public async Task<Device?> GetDeviceByFingerprintAsync(string fingerprint, CancellationToken ct = default)
        {
            return await _context.Devices
                .Include(d => d.License)
                .FirstOrDefaultAsync(d => d.DeviceFingerprint == fingerprint, ct);
        }

        public async Task<bool> UpdateDeviceLastSeenAsync(int deviceId, string clientIp, string appVersion, CancellationToken ct = default)
        {
            var device = await _context.Devices.FindAsync(new object[] { deviceId }, ct);
            if (device == null) return false;

            device.LastSeenAt = DateTime.UtcNow;
            device.LastIpAddress = clientIp;
            device.LastAppVersion = appVersion;

            return await _context.SaveChangesAsync(ct) > 0;
        }
    }
}