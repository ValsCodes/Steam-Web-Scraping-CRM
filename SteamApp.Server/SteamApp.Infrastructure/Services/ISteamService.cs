using SteamApp.Application.DTOs.WatchItem;
using SteamApp.Application.DTOs.WishListItem;

namespace SteamApp.Infrastructure.Services;

public interface ISteamService
{  
    Task<IEnumerable<WatchItemDto>> ScrapePage(long gamerUrlId, short page);

    Task<IEnumerable<WatchItemDto>> ScrapeFromPublicApi(long gameUrlId, short page);

    Task<IEnumerable<WatchItemDto>> ScrapeWithPixels(long gameUrlId, short page);

    Task<WhishListResponse?> CheckWishlistItem(long wishListId);

    // Not Done

    //Task<string> GetPixelInfoFromSource(long gamerUrlId, string srcUrl);

    //Task<WatchItemDto> ScrapeProductPixels(long gameId, string prodtucName);
}