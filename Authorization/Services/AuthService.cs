using HardwareInfoApis.Authorization.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace HardwareInfoApis.Authorization.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse?> LoginAsync(LoginRequest request);
    }

    public class AuthService : IAuthService
    {
        private readonly List<User> _users = new(); // In-memory store for example
        private readonly ILogger<AuthService> _logger;
        private readonly ITokenService _tokenService;
        private readonly JwtSettings _settings;

        public AuthService(IOptions<JwtSettings> options, ITokenService tokenService, ILogger<AuthService> logger)
        {
            _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _logger = logger;
        }

        public Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Email and password are required");

            if (_users.Any(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("User already exists");

            var salt = RandomNumberGenerator.GetBytes(16);
            var hash = HashPassword(request.Password, salt);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = hash,
                Salt = salt,
                CreatedAt = DateTime.UtcNow
            };

            _users.Add(user);

            var token = _tokenService.CreateToken(new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()), new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email) });

            return Task.FromResult(new AuthResponse(token, user.Id, user.Email));
        }

        public Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var user = _users.FirstOrDefault(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));
            if (user == null) return Task.FromResult<AuthResponse?>(null);

            var hash = HashPassword(request.Password, user.Salt);
            if (!hash.SequenceEqual(user.PasswordHash)) return Task.FromResult<AuthResponse?>(null);

            var token = _tokenService.CreateToken(new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()), new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email) });

            return Task.FromResult<AuthResponse?>(new AuthResponse(token, user.Id, user.Email));
        }

        private static byte[] HashPassword(string password, byte[] salt)
        {
            using var hmac = new HMACSHA256(salt);
            return hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
    }
}
