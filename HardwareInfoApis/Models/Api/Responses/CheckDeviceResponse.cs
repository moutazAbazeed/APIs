namespace HardwareInfoApis.Models.Api.Responses
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Response model for device check endpoint
    /// Returned when a client checks if a device is registered
    /// </summary>
    public class CheckDeviceResponse
    {
        /// <summary>
        /// Indicates if the device is registered in the system
        /// True = registered, False = not found
        /// </summary>
        [JsonPropertyName("isRegistered")]
        public bool IsRegistered { get; set; } = false;

        /// <summary>
        /// Unique device identifier (database ID)
        /// Only populated if device is registered
        /// </summary>
        [JsonPropertyName("deviceId")]
        public string? DeviceId { get; set; }

        /// <summary>
        /// Unique hardware fingerprint of the device (SHA256 hash)
        /// Echoed back for client verification
        /// </summary>
        [JsonPropertyName("deviceFingerprint")]
        public string? DeviceFingerprint { get; set; }

        /// <summary>
        /// Human-readable device name (from OS machine name or user-provided)
        /// </summary>
        [JsonPropertyName("deviceName")]
        public string? DeviceName { get; set; }

        /// <summary>
        /// Date and time when device was first registered (UTC)
        /// </summary>
        [JsonPropertyName("registrationDate")]
        public DateTime? RegistrationDate { get; set; }

        /// <summary>
        /// Date and time of last device activity/heartbeat (UTC)
        /// </summary>
        [JsonPropertyName("lastSeenAt")]
        public DateTime? LastSeenAt { get; set; }

        /// <summary>
        /// Device status (Active, Blocked, Pending, Expired, Inactive)
        /// </summary>
        [JsonPropertyName("deviceStatus")]
        public string? DeviceStatus { get; set; }

        /// <summary>
        /// Indicates if device is currently active and allowed to operate
        /// </summary>
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; } = false;

        /// <summary>
        /// Indicates if device has been blocked by admin
        /// </summary>
        [JsonPropertyName("isBlocked")]
        public bool IsBlocked { get; set; } = false;

        /// <summary>
        /// Reason for blocking (if applicable)
        /// </summary>
        [JsonPropertyName("blockReason")]
        public string? BlockReason { get; set; }

        // ═══════════════════════════════════════════════════════════
        // LICENSE INFORMATION
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// License status (Active, Expired, Revoked, None, Trial)
        /// </summary>
        [JsonPropertyName("licenseStatus")]
        public string? LicenseStatus { get; set; }

        /// <summary>
        /// License key assigned to this device (masked for security)
        /// Example: "PRO-XXXX-YYYY-ZZZZ"
        /// </summary>
        [JsonPropertyName("licenseKey")]
        public string? LicenseKey { get; set; }

        /// <summary>
        /// License type (Trial, Standard, Professional, Enterprise, Lifetime)
        /// </summary>
        [JsonPropertyName("licenseType")]
        public string? LicenseType { get; set; }

        /// <summary>
        /// License expiration date (UTC)
        /// Null for perpetual licenses or if no license
        /// </summary>
        [JsonPropertyName("licenseExpiryDate")]
        public DateTime? LicenseExpiryDate { get; set; }

        /// <summary>
        /// Number of days until license expires
        /// Null if perpetual, negative if expired
        /// </summary>
        [JsonPropertyName("daysUntilExpiry")]
        public int? DaysUntilExpiry
        {
            get
            {
                if (!LicenseExpiryDate.HasValue)
                    return null;

                return (LicenseExpiryDate.Value - DateTime.UtcNow).Days;
            }
        }

        /// <summary>
        /// Indicates if license is in grace period after expiration
        /// </summary>
        [JsonPropertyName("isInGracePeriod")]
        public bool IsInGracePeriod { get; set; } = false;

        /// <summary>
        /// Grace period end date (UTC)
        /// </summary>
        [JsonPropertyName("gracePeriodEndsAt")]
        public DateTime? GracePeriodEndsAt { get; set; }

        // ═══════════════════════════════════════════════════════════
        // DEVICE HEALTH & COMPLIANCE
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Indicates if device requires a heartbeat/keep-alive ping
        /// </summary>
        [JsonPropertyName("requiresHeartbeat")]
        public bool RequiresHeartbeat { get; set; } = false;

        /// <summary>
        /// Heartbeat interval in minutes (if required)
        /// </summary>
        [JsonPropertyName("heartbeatIntervalMinutes")]
        public int HeartbeatIntervalMinutes { get; set; } = 0;

        /// <summary>
        /// Indicates if device hardware has changed significantly
        /// May require re-validation
        /// </summary>
        [JsonPropertyName("hardwareChanged")]
        public bool HardwareChanged { get; set; } = false;

        /// <summary>
        /// Indicates if device requires software update
        /// </summary>
        [JsonPropertyName("requiresUpdate")]
        public bool RequiresUpdate { get; set; } = false;

        /// <summary>
        /// Minimum required app version (if update required)
        /// </summary>
        [JsonPropertyName("minimumAppVersion")]
        public string? MinimumAppVersion { get; set; }

        /// <summary>
        /// Latest app version available
        /// </summary>
        [JsonPropertyName("latestAppVersion")]
        public string? LatestAppVersion { get; set; }

        /// <summary>
        /// Update download URL (if update required)
        /// </summary>
        [JsonPropertyName("updateUrl")]
        public string? UpdateUrl { get; set; }

        // ═══════════════════════════════════════════════════════════
        // ACTIONS & INSTRUCTIONS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Actions the client should take based on this response
        /// Examples: "none", "renew_license", "update_app", "contact_support"
        /// </summary>
        [JsonPropertyName("actions")]
        public string[]? Actions { get; set; }

        /// <summary>
        /// User-friendly message to display
        /// </summary>
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        /// <summary>
        /// Warning messages (non-blocking)
        /// </summary>
        [JsonPropertyName("warnings")]
        public string[]? Warnings { get; set; }

        // ═══════════════════════════════════════════════════════════
        // METADATA
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Server timestamp for response (UTC)
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// API version that processed this request
        /// </summary>
        [JsonPropertyName("apiVersion")]
        public string ApiVersion { get; set; } = "1.0";

        /// <summary>
        /// Request ID for tracing/debugging (echoed from request)
        /// </summary>
        [JsonPropertyName("requestId")]
        public string? RequestId { get; set; }

        /// <summary>
        /// Additional extensible metadata
        /// </summary>
        [JsonPropertyName("metadata")]
        public object? Metadata { get; set; }
    }
}