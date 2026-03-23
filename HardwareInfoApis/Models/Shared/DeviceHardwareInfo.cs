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




    }
}