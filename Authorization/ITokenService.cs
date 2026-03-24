using System.Security.Claims;

namespace HardwareInfoApis.Authorization
{
    public interface ITokenService
    {
        string CreateToken(IEnumerable<Claim> claims);
        ClaimsPrincipal? ValidateToken(string token);
    }
}
