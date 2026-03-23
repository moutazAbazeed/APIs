namespace HardwareInfoApis.Api.Models
{
    public class ApiSettings
    {
        // General application settings
        public string? ApplicationName { get; set; }
        public string? AllowedOrigins { get; set; }

        // License / device related defaults
        public bool RequireLicenseKey { get; set; } = false;
        public int DefaultMaxDevicesPerLicense { get; set; } = 1;
        public int DeviceInactivityDays { get; set; } = 30;

        // Optional nested settings (expand as needed)
        public LoggingSettings? Logging { get; set; }
    }

    public class LoggingSettings
    {
        public string? Level { get; set; }
        public string? FilePath { get; set; }
    }
}