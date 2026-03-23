using System.Text.Json.Serialization;

namespace HardwareInfoApis.Models.Shared
{
    public class MemoryInfo
    {
        [JsonPropertyName("totalPhysicalRam")]
        public string TotalPhysicalRam { get; set; } = "Unknown";

        [JsonPropertyName("totalPhysicalBytes")]
        public long TotalPhysicalBytes { get; set; } = -1;

        [JsonPropertyName("availableBytes")]
        public long AvailableBytes { get; set; } = -1;

        // ✅ NEW: RAM Type (DDR3, DDR4, DDR5, etc.)
        [JsonPropertyName("memoryType")]
        public string MemoryType { get; set; } = "Unknown";

        // ✅ NEW: RAM Speed (MHz)
        [JsonPropertyName("memorySpeed")]
        public string MemorySpeed { get; set; } = "Unknown";

        // ✅ NEW: Memory Form Factor (SODIMM, DIMM, etc.)
        [JsonPropertyName("formFactor")]
        public string FormFactor { get; set; } = "Unknown";

        // ✅ NEW: Number of RAM Sticks
        [JsonPropertyName("memorySticks")]
        public int MemorySticks { get; set; } = 0;

        // ✅ NEW: Detailed info for each RAM module
        [JsonPropertyName("memoryModules")]
        public List<MemoryModuleInfo> MemoryModules { get; set; } = new List<MemoryModuleInfo>();

        [JsonIgnore]
        public double? UsedPercentage
        {
            get
            {
                if (TotalPhysicalBytes > 0 && AvailableBytes >= 0)
                {
                    var used = TotalPhysicalBytes - AvailableBytes;
                    return Math.Round((used / (double)TotalPhysicalBytes) * 100, 1);
                }
                return null;
            }
        }

        public override string ToString() =>
            $"{TotalPhysicalRam} {MemoryType} ({MemorySpeed})";
    }

    /// <summary>
    /// Detailed info for a single RAM module
    /// </summary>
    public class MemoryModuleInfo
    {
        [JsonPropertyName("manufacturer")]
        public string Manufacturer { get; set; } = "Unknown";

        [JsonPropertyName("partNumber")]
        public string PartNumber { get; set; } = "Unknown";

        [JsonPropertyName("serialNumber")]
        public string SerialNumber { get; set; } = "Unknown";

        [JsonPropertyName("capacity")]
        public string Capacity { get; set; } = "Unknown";

        [JsonPropertyName("capacityBytes")]
        public long CapacityBytes { get; set; } = 0;

        [JsonPropertyName("speed")]
        public string Speed { get; set; } = "Unknown";

        [JsonPropertyName("memoryType")]
        public string MemoryType { get; set; } = "Unknown";

        [JsonPropertyName("formFactor")]
        public string FormFactor { get; set; } = "Unknown";

        [JsonPropertyName("bankLabel")]
        public string BankLabel { get; set; } = "Unknown";

        [JsonIgnore]
        public string FormattedCapacity =>
            CapacityBytes > 0 ? $"{CapacityBytes / (1024.0 * 1024 * 1024):F0} GB" : "Unknown";
    }
}