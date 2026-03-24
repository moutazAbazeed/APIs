using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HardwareInfoApis.Authorization
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _settings;
        private readonly byte[] _keyBytes;
        private readonly ILogger<TokenService>? _logger;

        public TokenService(IOptions<JwtSettings> options, ILogger<TokenService>? logger = null)
        {
            _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrWhiteSpace(_settings.SigningKey))
                throw new ArgumentException("JWT signing key must be configured in JwtSettings.SigningKey");

            _keyBytes = Encoding.UTF8.GetBytes(_settings.SigningKey);
            _logger = logger;
        }

        public string CreateToken(IEnumerable<Claim> claims)
        {
            var signingKey = new SymmetricSecurityKey(_keyBytes);
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_settings.TokenLifetimeMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = !string.IsNullOrWhiteSpace(_settings.Issuer),
                    ValidIssuer = _settings.Issuer,
                    ValidateAudience = !string.IsNullOrWhiteSpace(_settings.Audience),
                    ValidAudience = _settings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(_keyBytes),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };

                var principal = handler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch (SecurityTokenException ex)
            {
                _logger?.LogWarning(ex, "Token validation failed: {Message}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error validating token");
                return null;
            }
        }
    }
}
