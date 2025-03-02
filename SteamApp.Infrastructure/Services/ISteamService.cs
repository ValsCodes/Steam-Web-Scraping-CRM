using SteamApp.Models.Dto;

namespace SteamApp.Infrastructure.Services
{
    public interface ISteamService
    {
        Task<ListingDto[]> GetFilterredListingsAsync(short page);
        Task<IEnumerable<ListingDto>> GetPaintedListingsOnlyAsync(short page);
        Task<(bool, string)> IsListingPaintedAsync(string name);

        Task<string[]> GetWeaponListingsUrls(short fromIndex, short batchSize);

        Task<string[]> GetHatListingsUrls(short fromPage, short batchSize);
    }
}
