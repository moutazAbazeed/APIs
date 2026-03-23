namespace HardwareInfoApis.Api.Models.Api
{
    public enum ApiErrorCode
    {
        // Client errors (1xxx)
        None = 0,
        InvalidRequest = 1000,
        InvalidFingerprint = 1001,
        InvalidLicenseKey = 1002,
        LicenseExpired = 1003,
        LicenseRevoked = 1004,
        DeviceLimitReached = 1005,
        DeviceAlreadyRegistered = 1006,
        DeviceBlocked = 1007,
        LicenseInvalid = 1008,
        // Server errors (2xxx)
        ServerError = 2000,
        DatabaseError = 2001,
        ConfigurationError = 2002,

        // Auth errors (3xxx)
        Unauthorized = 3000,
        Forbidden = 3001,
        InvalidToken = 3002,
        TokenExpired = 3003,

        // Rate limiting (4xxx)
        RateLimited = 4000,
        QuotaExceeded = 4001
    }
}