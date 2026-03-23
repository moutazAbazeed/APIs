using System;

namespace HardwareInfoApis.Models.Api.Responses
{
    /// <summary>
    /// Result returned from license activation operations.
    /// </summary>
    public class LicenseActivationResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ErrorCode { get; set; }

        /// <summary>
        /// When the license was activated for the device (or previously registered time).
        /// </summary>
        public DateTime? ActivatedAt { get; set; }

        /// <summary>
        /// License expiration date, if any.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }
}