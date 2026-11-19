using SteamApp.Application.Caching;
using SteamApp.Contracts.WatchItem;

namespace SteamApp.Application.Features.Scraping;

public static class ScrapeOperation
{
    public const string ScrapePageEndpoint = "scrape-page";
    public const string ScrapePublicApiEndpoint = "scrape-public-api";
    public const string ScrapePixelsEndpoint = "scrape-pixels";

    public const string WebScrapeType = "Web Scrape";
    public const string PublicApiScrapeType = "Public API Scrape";
    public const string PixelScrapeType = "Pixel Scrape";

    public static string GetScrapeType(string endpoint)
    {
        return endpoint switch
        {
            ScrapePageEndpoint => WebScrapeType,
            ScrapePublicApiEndpoint => PublicApiScrapeType,
            ScrapePixelsEndpoint => PixelScrapeType,
            _ => throw new InvalidOperationException("Unsupported scrape history endpoint.")
        };
    }

    public static string GetCacheKey(string endpoint, long gameUrlId, short page)
    {
        return endpoint switch
        {
            ScrapePageEndpoint => string.Format(CacheKeys.ScrapePage, gameUrlId, page),
            ScrapePublicApiEndpoint => string.Format(CacheKeys.ScrapePublic, gameUrlId, page),
            ScrapePixelsEndpoint => string.Format(CacheKeys.ScrapePixels, gameUrlId, page),
            _ => throw new InvalidOperationException("Unsupported scrape history endpoint.")
        };
    }

    public static IReadOnlyList<WatchItemDto> MaterializeResultsIfNeeded(IEnumerable<WatchItemDto> results)
    {
        return results switch
        {
            IReadOnlyList<WatchItemDto> readOnlyList => readOnlyList,
            ICollection<WatchItemDto> collection => collection.ToList(),
            _ => results.ToList()
        };
    }
}
