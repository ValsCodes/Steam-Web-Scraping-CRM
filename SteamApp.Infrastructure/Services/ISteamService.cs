using SteamApp.Infrastructure.DTOs;

namespace SteamApp.Infrastructure.Services
{
    public interface ISteamService
    {
        IEnumerable<string> GetWeaponBatchUrls(short fromIndex, short batchSize);

        IEnumerable<string> GetHatBatchUrls(short fromPage, short batchSize);

        Task<IEnumerable<ListingDto>> GetFilteredBulkListingsAsync(short page);

        Task<IEnumerable<ListingDto>> ScrapePageAsync(short page);

        Task<IEnumerable<PaintedListingsDto>> ScrapePageForPaintedListingsOnlyAsync(short page);

        Task<PaintedListingDto> CheckIsListingPaintedAsync(string name);
    }
}
