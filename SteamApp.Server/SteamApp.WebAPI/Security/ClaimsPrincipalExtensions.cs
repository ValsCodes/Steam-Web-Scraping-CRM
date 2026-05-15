using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SteamApp.WebAPI.Security;

public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
    }
}
