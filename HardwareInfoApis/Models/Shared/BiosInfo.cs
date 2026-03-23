using System.Text.Json.Serialization;

namespace HardwareInfoApis.Models.Shared
{
    public class BiosInfo
    {
        [JsonPropertyName("serialNumber")]
        public string SerialNumber { get; set; } = "Unknown";

        [JsonPropertyName("version")]
        public string Version { get; set; } = "Unknown";

        [JsonPropertyName("manufacturer")]
        public string Manufacturer { get; set; } = "Unknown";

        [JsonPropertyName("releaseDate")]
        public string ReleaseDate { get; set; } = "Unknown";

        public override string ToString() =>
            $"{Manufacturer} BIOS {Version} (S/N: {SerialNumber})";
    }
}