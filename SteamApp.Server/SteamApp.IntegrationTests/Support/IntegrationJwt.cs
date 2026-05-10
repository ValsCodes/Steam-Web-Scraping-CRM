using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SteamApp.WebAPI.Security;

namespace SteamApp.IntegrationTests.Support;

public static class IntegrationJwt
{
    public const string Key = "integration-test-jwt-key-32-bytes!!";
    public const string Issuer = "steamapp.integration.tests";
    public const string Audience = "steamapp.integration.tests";

    public static string CreateToken(
        string scope = SecurityPolicies.UserScope,
        DateTime? expiresUtc = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var now = DateTime.UtcNow;

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "integration-user-id"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim(ClaimTypes.NameIdentifier, "integration-user-id"),
            new Claim(ClaimTypes.Name, "Integration User"),
            new Claim("scope", scope)
        };

        var expires = expiresUtc ?? now.AddMinutes(60);
        var notBefore = expires < now
            ? expires.AddMinutes(-5)
            : now.AddMinutes(-1);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            notBefore: notBefore,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
