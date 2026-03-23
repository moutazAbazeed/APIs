namespace HardwareInfoApis.Models.Api.Responses
{
    using HardwareInfoApis.Api.Models.Entities;
    using System;

    public class LicenseStatistics
    {
        public string LicenseKey { get; set; }
        public LicenseType LicenseType { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int TotalActivations { get; set; }
        public int CurrentDeviceCount { get; set; }
        public int MaxDevices { get; set; }
        public int TotalValidations { get; set; }
        public int FailedValidations { get; set; }
        public DateTime? LastValidatedAt { get; set; }
        public DateTime? LastActivityAt { get; set; }
    }
}