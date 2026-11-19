using SteamApp.Contracts.WatchItem;
using SteamApp.Application.Abstractions.Services;

namespace SteamApp.Application.Features.Scraping;

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
            ScrapeOperation.ScrapePageEndpoint => await steamService.ScrapePage(gameUrlId, page),
            ScrapeOperation.ScrapePublicApiEndpoint => await steamService.ScrapeFromPublicApi(gameUrlId, page),
            ScrapeOperation.ScrapePixelsEndpoint => await steamService.ScrapeWithPixels(gameUrlId, page),
            _ => throw new InvalidOperationException("Unsupported scrape history endpoint.")
        };

        return ScrapeOperation.MaterializeResultsIfNeeded(results);
    }
}
