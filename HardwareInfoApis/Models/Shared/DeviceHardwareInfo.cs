using System;
using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HardwareInfoApis.Models.Shared
{
    /// <summary>
    /// Complete hardware information model for a device
    /// Suitable for JSON serialization, database storage, and API transfer
    /// </summary>
    public class DeviceHardwareInfo
    {
        #region Metadata

        /// <summary>
        /// Unique identifier for this hardware record
        /// </summary>
        [JsonPropertyName("recordId")]
        public Guid RecordId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Timestamp when hardware info was collected
        /// </summary>
        [JsonPropertyName("collectedAt")]
        public DateTime CollectedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Application/version that collected this info
        /// </summary>
        [JsonPropertyName("collectorApp")]
        public string CollectorApp { get; set; } = "ReportsApp v1.0";

        #endregion

        #region Processor

        [JsonPropertyName("processor")]
        public ProcessorInfo Processor { get; set; } = new ProcessorInfo();

        #endregion

        #region Memory

        [JsonPropertyName("memory")]
        public MemoryInfo Memory { get; set; } = new MemoryInfo();

        #endregion

        #region BIOS/Firmware

        [JsonPropertyName("bios")]
        public BiosInfo Bios { get; set; } = new BiosInfo();

        #endregion

        #region Storage

        [JsonPropertyName("storage")]
        public StorageInfo Storage { get; set; } = new StorageInfo();

        #endregion

        #region Operating System

        [JsonPropertyName("operatingSystem")]
        public OsInfo OperatingSystem { get; set; } = new OsInfo();

        #endregion

        #region Computed Values

        /// <summary>
        /// SHA256 hash of combined hardware identifiers
        /// Used for device fingerprinting and comparison
        /// </summary>
        [JsonPropertyName("deviceFingerprint")]
        public string DeviceFingerprint { get; set; }

        /// <summary>
        /// Raw (unhashed) fingerprint components - for debugging only
        /// ⚠️ Do not store or transmit in production
        /// </summary>
        [JsonIgnore]
        public string RawFingerprintComponents { get; private set; }

        #endregion

        #region Constructors

        public DeviceHardwareInfo() { }

        /// <summary>
        /// Collects hardware info from the current machine
        /// </summary>
        public static DeviceHardwareInfo CollectFromCurrentMachine(bool includeRawComponents = false)
        {
            var info = new DeviceHardwareInfo
            {
                Processor = new ProcessorInfo
                {
                    ProcessorId = Helpers.HardwareInfo.GetProcessorId(),
                    Model = Helpers.HardwareInfo.GetCpuModel(),
                    CoreCount = Environment.ProcessorCount
                },
                Memory = new MemoryInfo
                {
                    TotalPhysicalRam = Helpers.HardwareInfo.GetTotalRam(),
                    TotalPhysicalBytes = GetTotalRamBytes()
                },
                Bios = new BiosInfo
                {
                    SerialNumber = Helpers.HardwareInfo.GetBiosSerialNumber(),
                    Version = Helpers.HardwareInfo.GetBiosVersion()
                },
                Storage = new StorageInfo
                {
                    PrimaryDiskSerial = Helpers.HardwareInfo.GetDiskSerialNumber(),
                    PrimaryDiskModel = Helpers.HardwareInfo.GetDiskModel(),
                    TotalDiskSize = Helpers.HardwareInfo.GetDiskSize(),
                    TotalDiskBytes = GetTotalDiskBytes()
                },
                OperatingSystem = new OsInfo
                {
                    Name =  Helpers.HardwareInfo.GetWindowsVersion(),
                    InstallDate = Helpers.HardwareInfo.GetWindowsInstallDate(),
                    MachineName = Environment.MachineName,
                    UserName = Environment.UserName
                }
            };

            // Generate fingerprint
            info.GenerateFingerprint(includeRawComponents);

            return info;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Generates SHA256 fingerprint from hardware identifiers
        /// </summary>
        public void GenerateFingerprint(bool includeRawComponents = false)
        {
            var components = string.Join("|",
                $"CPU_ID:{Processor.ProcessorId}",
                $"CPU_MODEL:{Processor.Model}",
                $"BIOS_SERIAL:{Bios.SerialNumber}",
                $"DISK_SERIAL:{Storage.PrimaryDiskSerial}",
                $"RAM:{Memory.TotalPhysicalBytes}",
                $"DISK_SIZE:{Storage.TotalDiskBytes}"
            );

            if (includeRawComponents)
                RawFingerprintComponents = components;

            DeviceFingerprint = ComputeSha256Hash(components);
        }

        /// <summary>
        /// Compares this hardware info with another to detect changes
        /// </summary>
        /// <returns>True if hardware matches (within tolerance)</returns>
        public bool Matches(DeviceHardwareInfo other, HardwareMatchTolerance tolerance = HardwareMatchTolerance.Strict)
        {
            if (other == null) return false;

            // Always compare fingerprints first (fast path)
            if (!string.IsNullOrEmpty(DeviceFingerprint) &&
                !string.IsNullOrEmpty(other.DeviceFingerprint))
            {
                if (DeviceFingerprint == other.DeviceFingerprint)
                    return true;

                // If fingerprints don't match, do detailed comparison based on tolerance
            }

            // Strict: All critical identifiers must match exactly
            if (tolerance == HardwareMatchTolerance.Strict)
            {
                return Processor.ProcessorId == other.Processor.ProcessorId &&
                       Bios.SerialNumber == other.Bios.SerialNumber &&
                       Storage.PrimaryDiskSerial == other.Storage.PrimaryDiskSerial;
            }

            // Moderate: Allow OS reinstall (ignore OS info), require hardware match
            if (tolerance == HardwareMatchTolerance.Moderate)
            {
                return Processor.ProcessorId == other.Processor.ProcessorId &&
                       (Bios.SerialNumber == other.Bios.SerialNumber ||
                        Storage.PrimaryDiskSerial == other.Storage.PrimaryDiskSerial);
            }

            // Lenient: Only require CPU match (for VMs or hardware upgrades)
            if (tolerance == HardwareMatchTolerance.Lenient)
            {
                return Processor.ProcessorId == other.Processor.ProcessorId ||
                       Processor.Model == other.Processor.Model;
            }

            return false;
        }

        /// <summary>
        /// Returns a summary string for logging/display
        /// </summary>
        public override string ToString()
        {
            return $"Device: {Processor.Model} | {Memory.TotalPhysicalRam} RAM | " +
                   $"Fingerprint: {DeviceFingerprint?.Substring(0, 16)}...";
        }

        #endregion

        #region Helpers

        private static string ComputeSha256Hash(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));

            var sb = new System.Text.StringBuilder();
            foreach (var b in bytes)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }

        private static long GetTotalRamBytes()
        {
            try
            {
                using var searcher = new System.Management.ManagementObjectSearcher(
                    "SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
                foreach (System.Management.ManagementObject obj in searcher.Get())
                {
                    if (obj["TotalPhysicalMemory"] != null)
                        return Convert.ToInt64(obj["TotalPhysicalMemory"]);
                }
            }
            catch { }
            return -1;
        }

        private static long GetTotalDiskBytes()
        {
            try
            {
                long total = 0;
                using var searcher = new System.Management.ManagementObjectSearcher(
                    "SELECT Size FROM Win32_DiskDrive WHERE MediaType='Fixed hard disk media'");
                foreach (System.Management.ManagementObject obj in searcher.Get())
                {
                    if (obj["Size"] != null)
                        total += Convert.ToInt64(obj["Size"]);
                }
                return total > 0 ? total : -1;
            }
            catch { }
            return -1;
        }

        #endregion
    }
}