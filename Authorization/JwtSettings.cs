namespace HardwareInfoApis.Authorization
{
    public class JwtSettings
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string SigningKey { get; set; } = string.Empty; // Symmetric key (HMAC)
        public int TokenLifetimeMinutes { get; set; } = 60;
    }
}
