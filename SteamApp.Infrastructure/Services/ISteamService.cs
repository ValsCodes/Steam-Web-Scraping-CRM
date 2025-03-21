using SteamApp.Models.Dto;

namespace SteamApp.Infrastructure.Services
{
    public interface ISteamService
    {
        Task<IEnumerable<ListingDto>> GetFilterredListingsAsync(short page);
        Task<IEnumerable<ListingDto>> GetPaintedListingsOnlyAsync(short page);
        Task<(bool, string)> IsListingPaintedAsync(string name);

        Task<IEnumerable<ListingDto>> ScrapePageAsync(short page);

        //Task<IEnumerable<string>> GetWeaponListingsUrls(short fromIndex, short batchSize);
        IEnumerable<string> GetWeaponListingsUrls(short fromIndex, short batchSize);

        //Task<IEnumerable<string>> GetHatListingsUrls(short fromPage, short batchSize);
        IEnumerable<string> GetHatListingsUrls(short fromPage, short batchSize);
    }
}
