using SteamApp.Models.Proxy;

namespace SteamApp.Infrastructure.Services
{
    public interface ISteamService
    {
        Task<ProductProxy[]> GetFilterredListingsAsync(short page);
        Task<IEnumerable<ProductProxy>> GetPaintedListingsOnlyAsync(short page);
        Task<(bool, string)> IsListingPaintedAsync(string name);

        Task<string[]> GetWeaponListingsUrls(short fromIndex, short batchSize);

        Task<string[]> GetHatListingsUrls(short fromPage, short batchSize);
    }
}
