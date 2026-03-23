using System.Text.Json.Serialization;

namespace HardwareInfoApis.Models.Shared
{
    public class OsInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "Unknown";

        [JsonPropertyName("version")]
        public string Version { get; set; } = "Unknown";

        [JsonPropertyName("buildNumber")]
        public string BuildNumber { get; set; } = "Unknown";

        [JsonPropertyName("installDate")]
        public string InstallDate { get; set; } = "Unknown";

        [JsonPropertyName("machineName")]
        public string MachineName { get; set; } = Environment.MachineName;

        [JsonPropertyName("userName")]
        public string UserName { get; set; } = Environment.UserName;

        [JsonPropertyName("is64Bit")]
        public bool Is64Bit { get; set; } = Environment.Is64BitOperatingSystem;

        [JsonPropertyName("userDomain")]
        public string UserDomain { get; set; } =
            string.IsNullOrEmpty(Environment.UserDomainName) ? null : Environment.UserDomainName;

        public override string ToString() =>
            $"{Name} on {MachineName}";
    }
}