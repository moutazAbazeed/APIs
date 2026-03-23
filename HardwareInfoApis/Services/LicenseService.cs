namespace HardwareInfoApis.Services
{
    using HardwareInfoApis.Api.Data;
    using HardwareInfoApis.Api.Models.Entities;
    using HardwareInfoApis.Data;
    using HardwareInfoApis.Services.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Service implementation for license management operations
    /// Matches License entity with LicenseKey as primary key
    /// </summary>
    public class LicenseService : ILicenseService
    {
        private readonly ApplicationDbContext _context;
        private readonly LicenseSettings _licenseSettings;
        private readonly ILogger<LicenseService> _logger;

        public LicenseService(
            ApplicationDbContext context,
            IOptions<LicenseSettings> licenseSettings,
            ILogger<LicenseService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _licenseSettings = licenseSettings?.Value ?? throw new ArgumentNullException(nameof(licenseSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region License Validation

        public async Task<LicenseValidationResult> ValidateLicenseAsync(
            string licenseKey,
            string? deviceFingerprint = null,
            CancellationToken ct = default)
        {
            try
            {
                // Validate format first (quick fail)
                if (!IsValidLicenseKeyFormat(licenseKey))
                {
                    return new LicenseValidationResult
                    {
                        IsValid = false,
                        Status = "Invalid",
                        ErrorMessage = "Invalid license key format"
                    };
                }

                // Find license by primary key (LicenseKey)
                var license = await _context.Licenses
                    .Include(l => l.Devices)
                    .FirstOrDefaultAsync(l => l.LicenseKey == licenseKey, ct);

                if (license == null)
                {
                    return new LicenseValidationResult
                    {
                        IsValid = false,
                        Status = "Invalid",
                        ErrorMessage = "License key not found"
                    };
                }

                // Check if revoked
                if (license.IsRevoked)
                {
                    return new LicenseValidationResult
                    {
                        IsValid = false,
                        Status = "Revoked",
                        ErrorMessage = $"License revoked: {license.RevokedReason}",
                        LicenseKey = MaskLicenseKey(license.LicenseKey)
                    };
                }

                // Check if active using computed property
                if (!license.IsActive)
                {
                    return new LicenseValidationResult
                    {
                        IsValid = false,
                        Status = "Inactive",
                        ErrorMessage = "License is not active",
                        LicenseKey = MaskLicenseKey(license.LicenseKey)
                    };
                }

                // Check expiration details
                var now = DateTime.UtcNow;
                var isExpired = license.ExpiresAt.HasValue && license.ExpiresAt.Value < now;

                // Check grace period
                var isInGracePeriod = false;
                DateTime? gracePeriodEndsAt = null;

                if (isExpired && _licenseSettings.EnableGracePeriod)
                {
                    gracePeriodEndsAt = license.ExpiresAt.Value.AddDays(_licenseSettings.GracePeriodDays);
                    isInGracePeriod = now < gracePeriodEndsAt;
                }

                if (isExpired && !isInGracePeriod)
                {
                    return new LicenseValidationResult
                    {
                        IsValid = false,
                        Status = "Expired",
                        LicenseKey = MaskLicenseKey(license.LicenseKey),
                        LicenseType = license.LicenseType,
                        ExpiresAt = license.ExpiresAt,
                        DaysUntilExpiry = license.ExpiresAt.HasValue
                            ? (int)(license.ExpiresAt.Value - now).TotalDays
                            : null,
                        ErrorMessage = "License has expired"
                    };
                }

                // Check device limit
                if (_licenseSettings.MaxDevicesPerLicense > 0 &&
                    license.CurrentDeviceCount >= _licenseSettings.MaxDevicesPerLicense)
                {
                    // Check if this device is already activated
                    var deviceActivated = !string.IsNullOrEmpty(deviceFingerprint) &&
                        license.Devices.Any(d => d.DeviceFingerprint == deviceFingerprint);

                    if (!deviceActivated)
                    {
                        return new LicenseValidationResult
                        {
                            IsValid = false,
                            Status = "DeviceLimitReached",
                            LicenseKey = MaskLicenseKey(license.LicenseKey),
                            LicenseType = license.LicenseType,
                            CurrentDeviceCount = license.CurrentDeviceCount,
                            MaxDevices = license.MaxDevices,
                            ErrorMessage = $"License device limit reached ({license.CurrentDeviceCount}/{license.MaxDevices})"
                        };
                    }
                }

                // Build warnings
                var warnings = new List<string>();
                if (isInGracePeriod)
                {
                    warnings.Add($"License expired. Grace period ends on {gracePeriodEndsAt:yyyy-MM-dd}");
                }
                else if (license.ExpiresAt.HasValue)
                {
                    var daysUntilExpiry = (license.ExpiresAt.Value - now).TotalDays;
                    if (daysUntilExpiry <= _licenseSettings.ExpiryWarningDays)
                    {
                        warnings.Add($"License expires in {(int)daysUntilExpiry} days");
                    }
                }

                // Success
                return new LicenseValidationResult
                {
                    IsValid = true,
                    Status = isInGracePeriod ? "Expired (Grace Period)" : "Active",
                    LicenseKey = MaskLicenseKey(license.LicenseKey),
                    LicenseType = license.LicenseType,
                    ExpiresAt = license.ExpiresAt,
                    DaysUntilExpiry = license.ExpiresAt.HasValue
                        ? (int)(license.ExpiresAt.Value - now).TotalDays
                        : null,
                    IsInGracePeriod = isInGracePeriod,
                    GracePeriodEndsAt = gracePeriodEndsAt,
                    CurrentDeviceCount = license.CurrentDeviceCount,
                    MaxDevices = license.MaxDevices,
                    Warnings = warnings.Count > 0 ? warnings.ToArray() : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating license: {LicenseKey}", licenseKey);
                return new LicenseValidationResult
                {
                    IsValid = false,
                    Status = "Error",
                    ErrorMessage = "License validation failed"
                };
            }
        }

        public async Task<bool> IsLicenseActiveAsync(
            string licenseKey,
            CancellationToken ct = default)
        {
            var license = await GetLicenseByKeyAsync(licenseKey, ct);
            return license?.IsActive == true;
        }

        #endregion

        #region License Management

        public async Task<License> CreateLicenseAsync(
            LicenseType licenseType,
            int? expiryDays = null,
            int maxDevices = 1,
            string? customerEmail = null,
            string? issuedBy = null,
            CancellationToken ct = default)
        {
            // Generate unique license key
            string licenseKey;
            do
            {
                licenseKey = GenerateLicenseKey(licenseType);
            } while (await _context.Licenses.FindAsync(new object[] { licenseKey }, ct) != null);

            // Calculate expiry
            DateTime? expiresAt = null;
            if (expiryDays.HasValue && licenseType != LicenseType.Lifetime)
            {
                expiresAt = DateTime.UtcNow.AddDays(expiryDays.Value);
            }
            else if (licenseType == LicenseType.Trial)
            {
                expiresAt = DateTime.UtcNow.AddDays(_licenseSettings.TrialDurationDays);
            }

            var license = new License
            {
                LicenseKey = licenseKey,  // Primary key
                LicenseName = $"{licenseType} License",
                LicenseType = licenseType,
                MaxDevices = maxDevices,
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                IssuedBy = issuedBy,
                CustomerEmail = customerEmail,
                IsRevoked = false,
                CurrentDeviceCount = 0
            };

            _context.Licenses.Add(license);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("License created: {LicenseKey} (Type: {LicenseType})",
                licenseKey, licenseType);

            return license;
        }

        public async Task<LicenseActivationResult> ActivateLicenseAsync(
            string licenseKey,
            string deviceFingerprint,
            int deviceId,
            CancellationToken ct = default)
        {
            try
            {
                // Find license by primary key
                var license = await _context.Licenses
                    .Include(l => l.Devices)
                    .FirstOrDefaultAsync(l => l.LicenseKey == licenseKey, ct);

                if (license == null)
                {
                    return new LicenseActivationResult
                    {
                        Success = false,
                        Message = "License key not found",
                        ErrorCode = "LicenseNotFound"
                    };
                }

                // Check license status using computed property
                if (!license.IsActive)
                {
                    return new LicenseActivationResult
                    {
                        Success = false,
                        Message = license.IsRevoked ? "License is revoked" : "License is not active",
                        ErrorCode = license.IsRevoked ? "LicenseRevoked" : "LicenseNotActive"
                    };
                }

                // Check if already activated on this device
                var existingDevice = license.Devices.FirstOrDefault(d => d.DeviceFingerprint == deviceFingerprint);
                if (existingDevice != null)
                {
                    return new LicenseActivationResult
                    {
                        Success = true,
                        Message = "License already activated on this device",
                        ActivatedAt = existingDevice.RegisteredAt,
                        ExpiresAt = license.ExpiresAt
                    };
                }

                // Check device limit
                if (_licenseSettings.MaxDevicesPerLicense > 0 &&
                    license.CurrentDeviceCount >= _licenseSettings.MaxDevicesPerLicense)
                {
                    return new LicenseActivationResult
                    {
                        Success = false,
                        Message = $"Device limit reached ({license.CurrentDeviceCount}/{license.MaxDevices})",
                        ErrorCode = "DeviceLimitReached"
                    };
                }

                // Find and update device
                var device = await _context.Devices.FindAsync(new object[] { deviceId }, ct);
                if (device == null)
                {
                    return new LicenseActivationResult
                    {
                        Success = false,
                        Message = "Device not found",
                        ErrorCode = "DeviceNotFound"
                    };
                }

                // Activate license on device
                device.LicenseKey = license.LicenseKey;  // Foreign key to License
                license.CurrentDeviceCount++;
                license.ActivatedAt ??= DateTime.UtcNow;

                await _context.SaveChangesAsync(ct);

                _logger.LogInformation("License activated: {LicenseKey} on device {DeviceId}",
                    licenseKey, deviceId);

                return new LicenseActivationResult
                {
                    Success = true,
                    Message = "License activated successfully",
                    ActivatedAt = DateTime.UtcNow,
                    ExpiresAt = license.ExpiresAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating license: {LicenseKey}", licenseKey);
                return new LicenseActivationResult
                {
                    Success = false,
                    Message = "Activation failed",
                    ErrorCode = "ActivationError"
                };
            }
        }

        public async Task<bool> DeactivateLicenseAsync(
            string licenseKey,
            string deviceFingerprint,
            CancellationToken ct = default)
        {
            var license = await _context.Licenses
                .Include(l => l.Devices)
                .FirstOrDefaultAsync(l => l.LicenseKey == licenseKey, ct);

            if (license == null)
                return false;

            var device = license.Devices.FirstOrDefault(d => d.DeviceFingerprint == deviceFingerprint);
            if (device == null)
                return false;

            // Remove license reference from device
            device.LicenseKey = null;
            license.CurrentDeviceCount = Math.Max(0, license.CurrentDeviceCount - 1);

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("License deactivated: {LicenseKey} from device {DeviceFingerprint}",
                licenseKey, deviceFingerprint);

            return true;
        }

        public async Task<bool> RevokeLicenseAsync(
            string licenseKey,
            string reason,
            string? revokedBy = null,
            CancellationToken ct = default)
        {
            var license = await GetLicenseByKeyAsync(licenseKey, ct);
            if (license == null)
                return false;

            license.IsRevoked = true;
            license.RevokedReason = reason;
            license.RevokedAt = DateTime.UtcNow;
            // Note: Your entity doesn't have RevokedBy property, so we skip it

            await _context.SaveChangesAsync(ct);

            _logger.LogWarning("License revoked: {LicenseKey} - Reason: {Reason}",
                licenseKey, reason);

            return true;
        }

        public async Task<License> ExtendLicenseAsync(
            string licenseKey,
            int additionalDays,
            CancellationToken ct = default)
        {
            var license = await GetLicenseByKeyAsync(licenseKey, ct);
            if (license == null)
                throw new KeyNotFoundException($"License not found: {licenseKey}");

            if (license.ExpiresAt.HasValue)
            {
                license.ExpiresAt = license.ExpiresAt.Value.AddDays(additionalDays);
            }
            else
            {
                license.ExpiresAt = DateTime.UtcNow.AddDays(additionalDays);
            }

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("License extended: {LicenseKey} by {AdditionalDays} days",
                licenseKey, additionalDays);

            return license;
        }

        #endregion

        #region License Queries

        public async Task<License?> GetLicenseByKeyAsync(
            string licenseKey,
            CancellationToken ct = default)
        {
            // Query by primary key (LicenseKey)
            return await _context.Licenses
                .Include(l => l.Devices)
                .FirstOrDefaultAsync(l => l.LicenseKey == licenseKey, ct);
        }

        public async Task<License?> GetLicenseByDeviceAsync(
            string deviceFingerprint,
            CancellationToken ct = default)
        {
            // Find device first, then get its license via foreign key
            var device = await _context.Devices
                .Include(d => d.License)  // Navigation property
                .FirstOrDefaultAsync(d => d.DeviceFingerprint == deviceFingerprint, ct);

            return device?.License;
        }

        public async Task<List<License>> GetUserLicensesAsync(
            string userId,
            CancellationToken ct = default)
        {
            return await _context.Licenses
                .Where(l => l.Devices.Any(d => d.UserId.ToString() == userId))
                .Include(l => l.Devices)
                .ToListAsync(ct);
        }

        public async Task<List<License>> GetActiveLicensesAsync(
            CancellationToken ct = default)
        {
            // Use computed IsActive property via client evaluation
            // Or filter manually for better performance:
            var now = DateTime.UtcNow;
            return await _context.Licenses
                .Include(l => l.Devices)
                .Where(l => !l.IsRevoked &&
                           (!l.ExpiresAt.HasValue || l.ExpiresAt.Value > now))
                .ToListAsync(ct);
        }

        public async Task<List<License>> GetLicensesExpiringSoonAsync(
            int daysThreshold = 30,
            CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var thresholdDate = now.AddDays(daysThreshold);

            return await _context.Licenses
                .Include(l => l.Devices)
                .Where(l => !l.IsRevoked &&
                           l.ExpiresAt.HasValue &&
                           l.ExpiresAt.Value > now &&
                           l.ExpiresAt.Value <= thresholdDate)
                .ToListAsync(ct);
        }

        #endregion

        #region License Statistics

        public async Task<LicenseStatistics> GetLicenseStatisticsAsync(
            string licenseKey,
            CancellationToken ct = default)
        {
            var license = await GetLicenseByKeyAsync(licenseKey, ct);

            if (license == null)
                throw new KeyNotFoundException($"License not found: {licenseKey}");

            return new LicenseStatistics
            {
                LicenseKey = MaskLicenseKey(license.LicenseKey),
                LicenseType = license.LicenseType,
                IssuedAt = license.IssuedAt,
                ActivatedAt = license.ActivatedAt,
                ExpiresAt = license.ExpiresAt,
                TotalActivations = license.CurrentDeviceCount,
                CurrentDeviceCount = license.CurrentDeviceCount,
                MaxDevices = license.MaxDevices,
                // These would require additional tracking tables
                TotalValidations = 0,
                FailedValidations = 0,
                LastValidatedAt = null,
                LastActivityAt = license.Devices.Max(d => (DateTime?)d.LastSeenAt)
            };
        }

        public async Task<Dictionary<LicenseType, int>> GetLicenseCountsByTypeAsync(
            CancellationToken ct = default)
        {
            return await _context.Licenses
                .GroupBy(l => l.LicenseType)
                .ToDictionaryAsync(g => g.Key, g => g.Count(), ct);
        }

        #endregion

        #region License Transfer

        public async Task<LicenseTransferResult> TransferLicenseAsync(
            string licenseKey,
            string fromDeviceFingerprint,
            string toDeviceFingerprint,
            CancellationToken ct = default)
        {
            if (!_licenseSettings.AllowLicenseTransfer)
            {
                return new LicenseTransferResult
                {
                    Success = false,
                    Message = "License transfer is not allowed",
                    ErrorCode = "TransferNotAllowed"
                };
            }

            var license = await GetLicenseByKeyAsync(licenseKey, ct);
            if (license == null)
            {
                return new LicenseTransferResult
                {
                    Success = false,
                    Message = "License not found",
                    ErrorCode = "LicenseNotFound"
                };
            }

            var fromDevice = license.Devices.FirstOrDefault(d => d.DeviceFingerprint == fromDeviceFingerprint);
            if (fromDevice == null)
            {
                return new LicenseTransferResult
                {
                    Success = false,
                    Message = "Source device not found",
                    ErrorCode = "SourceDeviceNotFound"
                };
            }

            var toDevice = await _context.Devices
                .FirstOrDefaultAsync(d => d.DeviceFingerprint == toDeviceFingerprint, ct);
            if (toDevice == null)
            {
                return new LicenseTransferResult
                {
                    Success = false,
                    Message = "Destination device not found",
                    ErrorCode = "DestinationDeviceNotFound"
                };
            }

            // Perform transfer
            fromDevice.LicenseKey = null;
            toDevice.LicenseKey = license.LicenseKey;

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("License transferred: {LicenseKey} from {FromDevice} to {ToDevice}",
                licenseKey, fromDeviceFingerprint, toDeviceFingerprint);

            return new LicenseTransferResult
            {
                Success = true,
                Message = "License transferred successfully",
                TransferredAt = DateTime.UtcNow,
                FromDevice = fromDeviceFingerprint,
                ToDevice = toDeviceFingerprint
            };
        }

        #endregion

        #region License Key Generation

        public string GenerateLicenseKey(LicenseType licenseType)
        {
            // Format: TYPE-XXXX-XXXX-XXXX (matches [MaxLength(50)])
            var prefix = licenseType switch
            {
                LicenseType.Trial => "TRL",
                LicenseType.Standard => "STD",
                LicenseType.Professional => "PRO",
                LicenseType.Enterprise => "ENT",
                LicenseType.Lifetime => "LFT",
                _ => "LIC"
            };

            // Generate 3 groups of 4 alphanumeric characters
            var group1 = GenerateRandomString(4);
            var group2 = GenerateRandomString(4);
            var group3 = GenerateRandomString(4);

            return $"{prefix}-{group1}-{group2}-{group3}";  // e.g., "PRO-ABC1-DEF2-GHI3" (19 chars < 50)
        }

        public bool IsValidLicenseKeyFormat(string licenseKey)
        {
            if (string.IsNullOrWhiteSpace(licenseKey))
                return false;

            // Match pattern from settings or default: XXX-XXXX-XXXX-XXXX
            var pattern = string.IsNullOrEmpty(_licenseSettings.LicenseKeyPattern)
                ? @"^[A-Z]{3}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}$"
                : _licenseSettings.LicenseKeyPattern;

            return Regex.IsMatch(licenseKey, pattern, RegexOptions.IgnoreCase);
        }

        private static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Exclude confusing chars
            var result = new StringBuilder(length);

            using var rng = RandomNumberGenerator.Create();
            var data = new byte[length];
            rng.GetBytes(data);

            foreach (var b in data)
            {
                result.Append(chars[b % chars.Length]);
            }

            return result.ToString();
        }

        private static string MaskLicenseKey(string licenseKey)
        {
            if (string.IsNullOrEmpty(licenseKey) || licenseKey.Length < 8)
                return licenseKey;

            // Show first 4 and last 4: "PRO-****-****-GHI3"
            return $"{licenseKey.Substring(0, 4)}****{licenseKey.Substring(licenseKey.Length - 4)}";
        }

        #endregion
    }
}