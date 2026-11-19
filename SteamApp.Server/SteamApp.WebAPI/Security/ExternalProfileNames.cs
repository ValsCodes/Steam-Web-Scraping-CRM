using System.Security.Claims;

namespace SteamApp.WebAPI.Security;

internal sealed record ExternalProfileNames(string? FirstName, string? LastName)
{
    public static ExternalProfileNames ReadGoogleClaims(ClaimsPrincipal principal) =>
        new(ReadName(principal, "given_name"), ReadName(principal, "family_name"));

    private static string? ReadName(ClaimsPrincipal principal, string claim)
    {
        var value = principal.FindFirstValue(claim)?.Trim();
        return value is { Length: > 0 and <= 100 } && !value.Any(char.IsControl) ? value : null;
    }
}
