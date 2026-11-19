using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
        DateTime? expiresUtc = null,
        DateTime? authenticationTimeUtc = null,
        bool includeAuthenticationTime = true,
        bool includeSessionStamp = true)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var now = DateTime.UtcNow;

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, IntegrationSeed.UserId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim(ClaimTypes.NameIdentifier, IntegrationSeed.UserId),
            new Claim(ClaimTypes.Name, "Integration User"),
            new Claim(ClaimTypes.Role, SecurityPolicies.AdminRole),
            new Claim("scope", scope)
        };

        if (includeSessionStamp)
        {
            claims.Add(new Claim("steamapp_session_stamp",
                Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(IntegrationSeed.UserSecurityStamp)))));
        }

        if (includeAuthenticationTime)
        {
            var authenticationTime = authenticationTimeUtc ?? now;
            claims.Add(new Claim(
                "auth_time",
                new DateTimeOffset(authenticationTime.ToUniversalTime()).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64));
        }

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
