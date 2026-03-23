using System;

namespace HardwareInfoApis.Models.Api.Responses
{
    /// <summary>
    /// Result returned from license transfer operations.
    /// </summary>
    public class LicenseTransferResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ErrorCode { get; set; }
        public DateTime? TransferredAt { get; set; }
        public string? FromDevice { get; set; }
        public string? ToDevice { get; set; }
    }
}