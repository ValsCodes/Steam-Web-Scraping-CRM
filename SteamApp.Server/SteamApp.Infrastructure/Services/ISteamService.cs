using SteamApp.Models.DTOs;

namespace SteamApp.Infrastructure.Services;


//  TODO add cancelation tokens
public interface ISteamService
{
    IEnumerable<string> GetWeaponBatchUrls(short fromIndex, short batchSize);

    IEnumerable<string> GetHatBatchUrls(short fromPage, short batchSize);

    Task<string> GetPaintInfoFromSource(string src);

    Task<IEnumerable<ListingDto>> GetFilteredBulkListings(short page);

    Task<IEnumerable<ListingDto>> ScrapePage(short page);

    Task<IEnumerable<ListingDto>> ScrapePageByPixel(short page, bool isGoodPaintsOnly = true);

    Task<IEnumerable<PaintedListingsDto>> ScrapePageForPaintedListingsOnly(short page);

    Task<PaintedListingDto> CheckIsListingPainted(string name);
}
