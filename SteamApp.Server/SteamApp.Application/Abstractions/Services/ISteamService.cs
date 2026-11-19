using SteamApp.Contracts.WatchItem;

namespace SteamApp.Application.Abstractions.Services;

public interface ISteamService
{  
    Task<IEnumerable<WatchItemDto>> ScrapePage(long gamerUrlId, short page, CancellationToken cancellationToken = default);

    Task<IEnumerable<WatchItemDto>> ScrapeFromPublicApi(long gameUrlId, short page, CancellationToken cancellationToken = default);

    Task<IEnumerable<WatchItemDto>> ScrapeWithPixels(long gameUrlId, short page, CancellationToken cancellationToken = default);

    // Not Done

    //Task<string> GetPixelInfoFromSource(long gamerUrlId, string srcUrl);

    //Task<WatchItemDto> ScrapeProductPixels(long gameId, string prodtucName);
}