namespace HardwareInfoApis.Models.Api.Responses
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Response model for device registration endpoint
    /// Returned when a device successfully registers with the API
    /// </summary>
    public class RegisterDeviceResponse
    {
        /// <summary>
        /// Unique identifier for the registered device (database ID)
        /// </summary>
        [JsonPropertyName("deviceId")]
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// Unique hardware fingerprint of the device (SHA256 hash)
        /// </summary>
        [JsonPropertyName("deviceFingerprint")]
        public string DeviceFingerprint { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable device name (from OS machine name)
        /// </summary>
        [JsonPropertyName("deviceName")]
        public string DeviceName { get; set; } = string.Empty;

        /// <summary>
        /// Date and time when device was first registered (UTC)
        /// </summary>
        [JsonPropertyName("registrationDate")]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// License key assigned to this device (if applicable)
        /// Null if no license was provided or required
        /// </summary>
        [JsonPropertyName("licenseKey")]
        public string? LicenseKey { get; set; }

        /// <summary>
        /// License expiration date (UTC)
        /// Null for perpetual licenses or if no license
        /// </summary>
        [JsonPropertyName("licenseExpiryDate")]
        public DateTime? LicenseExpiryDate { get; set; }

        /// <summary>
        /// License type (Trial, Standard, Professional, Enterprise, Lifetime)
        /// </summary>
        [JsonPropertyName("licenseType")]
        public string? LicenseType { get; set; }

        /// <summary>
        /// Indicates if this was a new registration or existing device
        /// True = newly registered, False = already registered
        /// </summary>
        [JsonPropertyName("isNewRegistration")]
        public bool IsNewRegistration { get; set; } = true;

        /// <summary>
        /// Device status (Active, Blocked, Pending, Expired)
        /// </summary>
        [JsonPropertyName("deviceStatus")]
        public string DeviceStatus { get; set; } = "Active";

        /// <summary>
        /// Number of days until license expires (null if perpetual)
        /// Negative value means expired
        /// </summary>
        [JsonPropertyName("daysUntilExpiry")]
        public int? DaysUntilExpiry
        {
            get
            {
                if (!LicenseExpiryDate.HasValue)
                    return null;

                var days = (LicenseExpiryDate.Value - DateTime.UtcNow).Days;
                return days;
            }
        }

        /// <summary>
        /// Indicates if device requires a heartbeat/keep-alive
        /// </summary>
        [JsonPropertyName("requiresHeartbeat")]
        public bool RequiresHeartbeat { get; set; } = true;

        /// <summary>
        /// Heartbeat interval in minutes (if required)
        /// </summary>
        [JsonPropertyName("heartbeatIntervalMinutes")]
        public int HeartbeatIntervalMinutes { get; set; } = 60;

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
        /// Optional: Message or instructions for the client
        /// </summary>
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        /// <summary>
        /// Optional: Actions the client should take
        /// </summary>
        [JsonPropertyName("actions")]
        public string[]? Actions { get; set; }

        /// <summary>
        /// Optional: Additional metadata (extensible)
        /// </summary>
        [JsonPropertyName("metadata")]
        public object? Metadata { get; set; }
    }
}