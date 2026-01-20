using SteamApp.Application.DTOs.WatchItem;

namespace SteamApp.Infrastructure.Services;

public interface ISteamService
{
    Task<string> GetPaintInfoFromSource(string src, CancellationToken cancellationToken);

    Task<IEnumerable<WatchItemDto>> GetDeserializedLisitngsFromUrl(short page, CancellationToken cancellationToken);

    Task<IEnumerable<WatchItemDto>> ScrapePage(short page, CancellationToken cancellationToken);

    Task<IEnumerable<WatchItemDto>> ScrapePageWithSrcPixelPaintCheck(short page, bool isGoodPaintsOnly, CancellationToken cancellationToken);

    Task<IEnumerable<WatchItemDto>> ScrapePageForPaintedListingsOnly(short page, CancellationToken cancellationToken);

    Task<WatchItemDto> CheckIsListingPainted(string name, CancellationToken cancellationToken);
}