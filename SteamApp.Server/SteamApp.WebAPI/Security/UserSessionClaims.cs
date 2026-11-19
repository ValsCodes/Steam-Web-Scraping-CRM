using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using SteamApp.Infrastructure.Identity;

namespace SteamApp.WebAPI.Security;

internal static class UserSessionClaims
{
    public const string Stamp = "steamapp_session_stamp";
    public const string Mfa = "steamapp_mfa";

    public static string StampDigest(string stamp) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(stamp)));

    public static bool Matches(ClaimsPrincipal principal, ApplicationUser user) =>
        user.SecurityStamp is { } stamp &&
        principal.FindFirstValue(Stamp) == StampDigest(stamp) &&
        principal.HasClaim(Mfa, "true") == user.TwoFactorEnabled;

    public static async Task ValidateAsync(TokenValidatedContext context)
    {
        if (context.Principal is not { } principal ||
            !principal.HasClaim("scope", SecurityPolicies.UserScope) ||
            principal.HasClaim(claim => claim.Type == "client_id"))
        {
            return;
        }

        var id = principal.GetUserId();
        var users = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        if (id == null || await users.FindByIdAsync(id) is not { } user || !Matches(principal, user))
        {
            context.Fail("The user session is no longer valid.");
        }
    }
}
