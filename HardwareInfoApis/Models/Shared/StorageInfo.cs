using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace HardwareInfoApis.Models.Shared
{
    public class StorageInfo
    {
        [JsonPropertyName("primaryDiskSerial")]
        public string PrimaryDiskSerial { get; set; } = "Unknown";

        [JsonPropertyName("primaryDiskModel")]
        public string PrimaryDiskModel { get; set; } = "Unknown";

        // ✅ NEW: Disk Type (SSD, HDD, NVMe, etc.)
        [JsonPropertyName("primaryDiskType")]
        public string PrimaryDiskType { get; set; } = "Unknown";

        [JsonPropertyName("totalDiskSize")]
        public string TotalDiskSize { get; set; } = "Unknown";

        [JsonPropertyName("totalDiskBytes")]
        public long TotalDiskBytes { get; set; } = -1;

        [JsonPropertyName("allDisks")]
        public List<DiskDriveInfo> AllDisks { get; set; } = new List<DiskDriveInfo>();

        public override string ToString() =>
            $"{PrimaryDiskType} - {PrimaryDiskModel} ({TotalDiskSize})";
    }

    public class DiskDriveInfo
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "Unknown";

        [JsonPropertyName("serialNumber")]
        public string SerialNumber { get; set; } = "Unknown";

        [JsonPropertyName("sizeBytes")]
        public long SizeBytes { get; set; } = 0;

        [JsonPropertyName("mediaType")]
        public string MediaType { get; set; } = "Unknown";

        [JsonPropertyName("interface")]
        public string Interface { get; set; } = "Unknown";

        // ✅ NEW: Disk Type (SSD, HDD, NVMe, etc.)
        [JsonPropertyName("diskType")]
        public string DiskType { get; set; } = "Unknown";

        [JsonIgnore]
        public string FormattedSize =>
            SizeBytes > 0 ? $"{SizeBytes / (1024.0 * 1024 * 1024):F0} GB" : "Unknown";
    }
}