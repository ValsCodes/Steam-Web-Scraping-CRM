namespace SteamApp.Application.Utilities;

public static class FeedbackRequestReference
{
    private const string Prefix = "FB-";

    public static string Format(long id)
    {
        return $"{Prefix}{id:000000}";
    }

    public static bool TryParse(string? value, out long id)
    {
        id = 0;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();
        if (!trimmed.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var rawId = trimmed[Prefix.Length..];
        return long.TryParse(rawId, out id) && id > 0;
    }
}
