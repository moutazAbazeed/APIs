using HardwareInfoApis.Models.Shared;
using HardwareInfoApis.Api.Services.Interfaces;
using Microsoft.Extensions.Logging;

using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HardwareInfoApis.Api.Services
{
    /// <summary>
    /// Service for validating and normalizing device fingerprints
    /// </summary>
    // Implement the interface defined in Api.Services.Interfaces
    public class FingerprintService : IFingerprintService
    {
        private readonly ILogger<FingerprintService> _logger;

        public FingerprintService(ILogger<FingerprintService> logger)
        {
            _logger = logger;
        }

        public bool IsValidFingerprint(string fingerprint)
        {
            if (string.IsNullOrWhiteSpace(fingerprint))
                return false;

            // SHA256 hash = 64 hex characters
            if (fingerprint.Length != 64)
                return false;

            // Check all characters are hex
            foreach (var c in fingerprint)
            {
                if (!char.IsDigit(c) && !(c >= 'a' && c <= 'f'))
                    return false;
            }

            return true;
        }

        public string NormalizeFingerprint(string fingerprint)
        {
            if (string.IsNullOrWhiteSpace(fingerprint))
                return string.Empty;

            // Convert to lowercase for consistency
            return fingerprint.Trim().ToLowerInvariant();
        }

        public Task<string> ComputeFingerprintAsync(DeviceHardwareInfo hardware)
        {
            if (hardware == null)
                return Task.FromResult(string.Empty);

            // Recreate the same fingerprint logic as the client
            var components = string.Join("|",
                $"CPU:{hardware.Processor?.ProcessorId}",
                $"MODEL:{hardware.Processor?.Model}",
                $"BIOS:{hardware.Bios?.SerialNumber}",
                $"DISK:{hardware.Storage?.PrimaryDiskSerial}",
                $"RAM:{hardware.Memory?.TotalPhysicalBytes}",
                $"DISK_SIZE:{hardware.Storage?.TotalDiskBytes}"
            );

            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(components));

            var sb = new StringBuilder();
            foreach (var b in bytes)
                sb.Append(b.ToString("x2"));

            return Task.FromResult(sb.ToString());
        }
    }
}