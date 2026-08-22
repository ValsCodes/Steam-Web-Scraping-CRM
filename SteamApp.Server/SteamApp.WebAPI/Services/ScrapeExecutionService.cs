using SteamApp.Application.DTOs.WatchItem;
using SteamApp.Interfaces.Services;
using SteamApp.WebAPI.Scraping;

namespace SteamApp.WebAPI.Services;

public sealed class ScrapeExecutionService(
    ISteamService steamService) : IScrapeExecutionService
{
    public async Task<IReadOnlyList<WatchItemDto>> ExecuteAsync(
        string endpoint,
        long gameUrlId,
        short page)
    {
        var results = endpoint switch
        {
            ScrapeEndpointDefinitions.ScrapePageEndpoint => await steamService.ScrapePage(gameUrlId, page),
            ScrapeEndpointDefinitions.ScrapePublicApiEndpoint => await steamService.ScrapeFromPublicApi(gameUrlId, page),
            ScrapeEndpointDefinitions.ScrapePixelsEndpoint => await steamService.ScrapeWithPixels(gameUrlId, page),
            _ => throw new InvalidOperationException("Unsupported scrape history endpoint.")
        };

        return ScrapeEndpointDefinitions.MaterializeResultsIfNeeded(results);
    }
}
