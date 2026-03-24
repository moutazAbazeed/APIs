using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace HardwareInfoApis.Authorization
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, JwtSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrWhiteSpace(settings.SigningKey))
                throw new ArgumentException("JwtSettings.SigningKey must be provided to enable JWT authentication.");

            services.Configure<JwtSettings>(_ =>
            {
                _.Issuer = settings.Issuer;
                _.Audience = settings.Audience;
                _.SigningKey = settings.SigningKey;
                _.TokenLifetimeMinutes = settings.TokenLifetimeMinutes;
            });

            var keyBytes = Encoding.UTF8.GetBytes(settings.SigningKey);

            services.AddSingleton<ITokenService, TokenService>();
            // Register auth service for login/register
            services.AddScoped<Services.IAuthService, Services.AuthService>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // In production RequireHttpsMetadata should be true
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = !string.IsNullOrWhiteSpace(settings.Issuer),
                    ValidIssuer = settings.Issuer,
                    ValidateAudience = !string.IsNullOrWhiteSpace(settings.Audience),
                    ValidAudience = settings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

            return services;
        }
    }
}
