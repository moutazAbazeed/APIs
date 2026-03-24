using System;
using System.Security.Claims;
using Xunit;
using HardwareInfoApis.Authorization;
using Microsoft.Extensions.Options;

namespace HardwareInfoApis.Tests
{
    public class TokenServiceTests
    {
        private readonly TokenService _service;

        public TokenServiceTests()
        {
            var settings = Options.Create(new JwtSettings
            {
                Issuer = "test-issuer",
                Audience = "test-audience",
                SigningKey = "super-secret-signing-key-which-is-long-enough",
                TokenLifetimeMinutes = 60
            });

            _service = new TokenService(settings);
        }

        [Fact]
        public void CreateToken_And_ValidateToken_Succeeds()
        {
            var claims = new[] { new Claim(ClaimTypes.Name, "tester") };
            var token = _service.CreateToken(claims);
            Assert.False(string.IsNullOrWhiteSpace(token));

            var principal = _service.ValidateToken(token);
            Assert.NotNull(principal);
            Assert.Equal("tester", principal.Identity?.Name);
        }

        [Fact]
        public void ValidateToken_ReturnsNull_ForInvalidToken()
        {
            var principal = _service.ValidateToken("not-a-token");
            Assert.Null(principal);
        }
    }
}
