using SteamApp.Application.DTOs.WatchItem;

namespace SteamApp.Infrastructure.Services;

public interface ISteamService
{
    Task<string> GetPixelInfoFromSource(long gamerUrlId, string srcUrl);

    Task<IEnumerable<WatchItemDto>> ScrapePage(long gamerUrlId, short page);

    Task<IEnumerable<WatchItemDto>> ScrapeFromPublicApi(long gameUrlId, short page);

    Task<IEnumerable<WatchItemDto>> ScrapeForPixels(long gameUrlId, short page);


    // Not Done
    Task<WatchItemDto> ScrapeProductPixels(long gameId, string prodtucName);
}