namespace HardwareInfoApis.Models.Api
{
    public class LicenseSettings
    {
        // Grace period settings
        public bool EnableGracePeriod { get; set; } = true;
        public int GracePeriodDays { get; set; } = 7;

        // Device / license limits and defaults
        public int MaxDevicesPerLicense { get; set; } = 5;
        public int ExpiryWarningDays { get; set; } = 14;
        public int TrialDurationDays { get; set; } = 14;

        // Transfer and pattern options
        public bool AllowLicenseTransfer { get; set; } = true;
        public string? LicenseKeyPattern { get; set; } = null;
    }
}