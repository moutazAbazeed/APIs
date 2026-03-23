namespace HardwareInfoApis.Models.Api.Requests
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Request model for device check endpoint
    /// Sent by client applications to verify if a device is registered
    /// </summary>
    public class CheckDeviceRequest
    {
        /// <summary>
        /// Unique hardware fingerprint of the device (SHA256 hash - 64 characters)
        /// This is the primary identifier used to look up the device
        /// </summary>
        [Required(ErrorMessage = "Device fingerprint is required")]
        [MinLength(64, ErrorMessage = "Device fingerprint must be 64 characters (SHA256)")]
        [MaxLength(64, ErrorMessage = "Device fingerprint must be 64 characters (SHA256)")]
        [RegularExpression(@"^[a-f0-9]{64}$", ErrorMessage = "Device fingerprint must be a valid SHA256 hash (lowercase hex)")]
        [JsonPropertyName("deviceFingerprint")]
        public string DeviceFingerprint { get; set; } = string.Empty;

        /// <summary>
        /// Application version making the request
        /// Used for compatibility checks and update notifications
        /// Format: Semantic versioning (e.g., "1.0.0")
        /// </summary>
        [Required(ErrorMessage = "App version is required")]
        [MinLength(1, ErrorMessage = "App version cannot be empty")]
        [MaxLength(50, ErrorMessage = "App version is too long")]
        [JsonPropertyName("appVersion")]
        public string AppVersion { get; set; } = string.Empty;

        /// <summary>
        /// Optional: License key to validate along with device check
        /// If provided, license status will be included in response
        /// </summary>
        [MaxLength(100, ErrorMessage = "License key is too long")]
        [JsonPropertyName("licenseKey")]
        public string? LicenseKey { get; set; }

        /// <summary>
        /// Optional: User ID associated with the device
        /// Used for account-based device management
        /// Format: GUID or external user identifier
        /// </summary>
        [JsonPropertyName("userId")]
        public string? UserId { get; set; }

        /// <summary>
        /// Optional: Request timestamp from client (for clock skew detection)
        /// Format: ISO 8601 UTC datetime
        /// </summary>
        [JsonPropertyName("clientTimestamp")]
        public DateTime? ClientTimestamp { get; set; }

        /// <summary>
        /// Optional: Platform/OS type
        /// Values: "Windows", "macOS", "Linux", "Android", "iOS"
        /// </summary>
        [JsonPropertyName("platform")]
        public string? Platform { get; set; }

        /// <summary>
        /// Optional: Platform version
        /// Example: "10.0.19045" for Windows 10
        /// </summary>
        [MaxLength(50, ErrorMessage = "Platform version is too long")]
        [JsonPropertyName("platformVersion")]
        public string? PlatformVersion { get; set; }

        /// <summary>
        /// Optional: API key for authenticated requests
        /// Alternative to JWT bearer token authentication
        /// </summary>
        [MaxLength(100, ErrorMessage = "API key is too long")]
        [JsonPropertyName("apiKey")]
        public string? ApiKey { get; set; }

        /// <summary>
        /// Optional: Include detailed hardware comparison in response
        /// If true, response will indicate if hardware has changed
        /// Default: false
        /// </summary>
        [JsonPropertyName("includeHardwareComparison")]
        public bool IncludeHardwareComparison { get; set; } = false;

        /// <summary>
        /// Optional: Update last seen timestamp on check
        /// If true, device's LastSeenAt will be updated
        /// Default: true (recommended for accurate tracking)
        /// </summary>
        [JsonPropertyName("updateLastSeen")]
        public bool UpdateLastSeen { get; set; } = true;

        /// <summary>
        /// Optional: Request ID for tracing/debugging
        /// Echoed back in response for correlation
        /// </summary>
        [JsonPropertyName("requestId")]
        public string? RequestId { get; set; }

        /// <summary>
        /// Optional: Additional metadata (extensible JSON object)
        /// </summary>
        [JsonPropertyName("metadata")]
        public object? Metadata { get; set; }
    }
}