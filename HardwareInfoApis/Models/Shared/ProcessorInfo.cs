using System.Text.Json.Serialization;

namespace HardwareInfoApis.Models.Shared
{
    public class ProcessorInfo
    {
        [JsonPropertyName("processorId")]
        public string ProcessorId { get; set; } = "Unknown";

        [JsonPropertyName("model")]
        public string Model { get; set; } = "Unknown";

        [JsonPropertyName("coreCount")]
        public int CoreCount { get; set; } = Environment.ProcessorCount;

        [JsonPropertyName("architecture")]
        public string Architecture { get; set; } =
            Environment.Is64BitProcess ? "x64" : "x86";

        public override string ToString() =>
            $"{Model} ({CoreCount} cores)";
    }
}