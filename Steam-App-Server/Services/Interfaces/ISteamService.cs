using SteamAppServer.Repositories.Interfaces;
using SteamAppServer.Models.Proxies;

namespace SteamAppServer.Services.Interfaces
{
    public interface ISteamService : ISalesRepository
    {
        Task<IEnumerable<ProductProxy>> GetFilterredListingsAsync(short page);
        Task<IEnumerable<ProductProxy>> GetPaintedListingsOnlyAsync(short page);
        Task<(bool, string)> IsListingPaintedAsync(string name);
    }
}
