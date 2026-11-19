using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace SteamApp.Infrastructure.ExternalServices;

internal static class SteamScrapeDestination
{
    private static readonly string[] CommunityHosts = ["steamcommunity.com", "www.steamcommunity.com"];
    private static readonly string[] AssetHosts =
    [
        "community.akamai.steamstatic.com",
        "community.cloudflare.steamstatic.com",
        "community.fastly.steamstatic.com",
        "avatars.akamai.steamstatic.com",
        "avatars.fastly.steamstatic.com",
        "shared.akamai.steamstatic.com",
        "shared.fastly.steamstatic.com",
        "cdn.akamai.steamstatic.com",
        "cdn.fastly.steamstatic.com"
    ];

    public static Uri RequireMarketUrl(string url)
    {
        var uri = ParseHttps(url);
        if (uri is null || !CommunityHosts.Contains(uri.IdnHost, StringComparer.OrdinalIgnoreCase) ||
            !uri.AbsolutePath.StartsWith("/market/", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The scrape URL must be an HTTPS Steam Community Market URL.");
        }

        return uri;
    }

    public static Uri RequireAssetUrl(string url)
    {
        var uri = ParseHttps(url);
        if (uri is null || !AssetHosts.Contains(uri.IdnHost, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The listing image must be hosted by a Steam asset server.");
        }

        return uri;
    }

    public static void ConfigureBrowser(ChromeOptions options)
    {
        var permittedHosts = CommunityHosts.Concat(AssetHosts).Select(host => $"EXCLUDE {host}");
        options.AddArgument($"--host-resolver-rules=MAP * ~NOTFOUND, {string.Join(", ", permittedHosts)}");
        options.AddArgument("--no-proxy-server");
        options.AddUserProfilePreference("profile.default_content_setting_values.popups", 2);
    }

    public static Task RestrictBrowserRequestsAsync(IWebDriver driver)
    {
        var network = driver.Manage().Network;
        network.AddRequestHandler(new NetworkRequestHandler
        {
            RequestMatcher = request =>
                ParseHttps(request.Url) is not { } uri ||
                (!CommunityHosts.Contains(uri.IdnHost, StringComparer.OrdinalIgnoreCase) &&
                    !AssetHosts.Contains(uri.IdnHost, StringComparer.OrdinalIgnoreCase)),
            ResponseSupplier = _ => new HttpResponseData { StatusCode = 403, Body = string.Empty }
        });
        return network.StartMonitoring();
    }

    private static Uri? ParseHttps(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
        uri.Scheme == Uri.UriSchemeHttps && uri.IsDefaultPort && uri.UserInfo.Length == 0
            ? uri
            : null;
}
