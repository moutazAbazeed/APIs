using System;

namespace HardwareInfoApis.Authorization.Models
{
    public record RegisterRequest(string Email, string Password);
    public record LoginRequest(string Email, string Password);
    public record AuthResponse(string Token, Guid UserId, string Email);

    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public byte[] PasswordHash { get; set; } = null!;
        public byte[] Salt { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
