using HardwareInfoApis.Models.Shared;
using HardwareInfoApis.Api.Services.Interfaces;
using Microsoft.Extensions.Logging;

using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
        private static readonly Regex Hex64Regex = new("^[0-9a-fA-F]{64}$", RegexOptions.Compiled);

        public FingerprintService(ILogger<FingerprintService> logger)
        {
            _logger = logger;
        }

        public bool IsValidFingerprint(string fingerprint)
        {
            if (string.IsNullOrWhiteSpace(fingerprint))
                return false;

            fingerprint = fingerprint.Trim();

            // Use a compiled regex to validate 64 hex chars (allow upper or lower case)
            var valid = Hex64Regex.IsMatch(fingerprint);
            if (!valid)
                _logger.LogDebug("Invalid fingerprint format: {FingerprintLength} chars", fingerprint.Length);

            return valid;
        }

        public string NormalizeFingerprint(string fingerprint)
        {
            if (string.IsNullOrWhiteSpace(fingerprint))
                return string.Empty;

            // Trim and convert to lowercase for a canonical representation
            return fingerprint.Trim().ToLowerInvariant();
        }

        public Task<string> ComputeFingerprintAsync(DeviceHardwareInfo hardware)
        {
            if (hardware == null)
                return Task.FromResult(string.Empty);

            // Build deterministic component string using safe accessors
            string cpuId = hardware.Processor?.ProcessorId ?? string.Empty;
            string cpuModel = hardware.Processor?.Model ?? string.Empty;
            string bios = hardware.Bios?.SerialNumber ?? string.Empty;
            string disk = hardware.Storage?.PrimaryDiskSerial ?? string.Empty;
            string ram = hardware.Memory != null ? hardware.Memory.TotalPhysicalBytes.ToString() : string.Empty;
            string diskSize = hardware.Storage != null ? hardware.Storage.TotalDiskBytes.ToString() : string.Empty;

            var components = string.Join("|",
                $"CPU:{cpuId}",
                $"MODEL:{cpuModel}",
                $"BIOS:{bios}",
                $"DISK:{disk}",
                $"RAM:{ram}",
                $"DISK_SIZE:{diskSize}"
            );

            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(components));

            var sb = new StringBuilder(64);
            foreach (var b in bytes)
                sb.Append(b.ToString("x2"));

            return Task.FromResult(sb.ToString());
        }
    }
}