using System;
using HardwareInfoApis.Api.Models.Entities;

namespace HardwareInfoApis.Models.Api.Responses
{
    /// <summary>
    /// Result returned from license validation operations.
    /// Contains status, metadata and optional warnings for a license check.
    /// </summary>
    public class LicenseValidationResult
    {
        public bool IsValid { get; set; }
        public string? Status { get; set; }
        public string? ErrorMessage { get; set; }

        // Masked license key for safe logging/returning
        public string? LicenseKey { get; set; }

        public LicenseType? LicenseType { get; set; }
        public DateTime? ExpiresAt { get; set; }

        // Number of whole days until expiry (can be negative)
        public int? DaysUntilExpiry { get; set; }

        // Grace period info
        public bool IsInGracePeriod { get; set; }
        public DateTime? GracePeriodEndsAt { get; set; }

        // Device usage
        public int? CurrentDeviceCount { get; set; }
        public int? MaxDevices { get; set; }

        // Optional warnings to return to caller
        public string[]? Warnings { get; set; }
    }
}