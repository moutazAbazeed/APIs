namespace HardwareInfoApis.Models.Api.Requests
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;
    using HardwareInfoApis.Models.Shared;

    /// <summary>
    /// Request model for device registration endpoint
    /// Sent by client applications to register a new device
    /// </summary>
    public class RegisterDeviceRequest
    {
        /// <summary>
        /// Unique hardware fingerprint of the device (SHA256 hash - 64 characters)
        /// This is the primary identifier for the device
        /// </summary>
        [Required(ErrorMessage = "Device fingerprint is required")]
        [MinLength(64, ErrorMessage = "Device fingerprint must be 64 characters (SHA256)")]
        [MaxLength(64, ErrorMessage = "Device fingerprint must be 64 characters (SHA256)")]
        [RegularExpression(@"^[a-f0-9]{64}$", ErrorMessage = "Device fingerprint must be a valid SHA256 hash (lowercase hex)")]
        [JsonPropertyName("deviceFingerprint")]
        public string DeviceFingerprint { get; set; } = string.Empty;

        /// <summary>
        /// Complete hardware information snapshot
        /// Contains CPU, RAM, BIOS, Storage, OS details
        /// </summary>
        [Required(ErrorMessage = "Hardware information is required")]
        [JsonPropertyName("hardwareInfo")]
        public DeviceHardwareInfo? HardwareInfo { get; set; }

        /// <summary>
        /// Application version making the request
        /// Used for compatibility checks and analytics
        /// </summary>
        [Required(ErrorMessage = "App version is required")]
        [MinLength(1, ErrorMessage = "App version cannot be empty")]
        [MaxLength(50, ErrorMessage = "App version is too long")]
        [JsonPropertyName("appVersion")]
        public string AppVersion { get; set; } = string.Empty;

        /// <summary>
        /// Optional: License key for activation
        /// If provided, device will be linked to this license
        /// </summary>
        [MaxLength(100, ErrorMessage = "License key is too long")]
        [JsonPropertyName("licenseKey")]
        public string? LicenseKey { get; set; }

        /// <summary>
        /// Optional: User ID to associate device with an account
        /// Format: GUID or external user identifier
        /// </summary>
        [JsonPropertyName("userId")]
        public string? UserId { get; set; }

        /// <summary>
        /// Optional: Email address for notifications
        /// Used for license expiry alerts, updates, etc.
        /// </summary>
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        [MaxLength(255, ErrorMessage = "Email address is too long")]
        [JsonPropertyName("userEmail")]
        public string? UserEmail { get; set; }

        /// <summary>
        /// Optional: Device alias/name provided by user
        /// If not provided, will use OS machine name from HardwareInfo
        /// </summary>
        [MaxLength(100, ErrorMessage = "Device name is too long")]
        [JsonPropertyName("deviceName")]
        public string? DeviceName { get; set; }

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
        /// Optional: Request timestamp from client (for clock skew detection)
        /// </summary>
        [JsonPropertyName("clientTimestamp")]
        public DateTime? ClientTimestamp { get; set; }

        /// <summary>
        /// Optional: API key for authenticated requests
        /// Alternative to JWT bearer token authentication
        /// </summary>
        [MaxLength(100, ErrorMessage = "API key is too long")]
        [JsonPropertyName("apiKey")]
        public string? ApiKey { get; set; }

        /// <summary>
        /// Optional: Additional metadata (extensible JSON object)
        /// </summary>
        [JsonPropertyName("metadata")]
        public object? Metadata { get; set; }

        /// <summary>
        /// Optional: Indicates if this is a re-registration/refresh
        /// True = refreshing existing registration, False = new registration
        /// </summary>
        [JsonPropertyName("isRefresh")]
        public bool IsRefresh { get; set; } = false;

        /// <summary>
        /// Optional: Request ID for tracing/debugging
        /// </summary>
        [JsonPropertyName("requestId")]
        public string? RequestId { get; set; }
    }
}