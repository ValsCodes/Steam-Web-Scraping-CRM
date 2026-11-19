using Microsoft.Extensions.Caching.Memory;
using SteamApp.Application.Features.Scraping;

namespace SteamApp.WebAPI.Scraping;

public static class ScrapeEndpointDefinitions
{
    public static bool TryGetCachedResults(
        IMemoryCache cache,
        string endpoint,
        long gameUrlId,
        short page,
        out object? cached)
    {
        return cache.TryGetValue(ScrapeOperation.GetCacheKey(endpoint, gameUrlId, page), out cached);
    }
}
