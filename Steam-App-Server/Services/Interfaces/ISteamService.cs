using SteamAppServer.Models;
using SteamAppServer.Models.Proxies;

namespace SteamAppServer.Services.Interfaces
{
    public interface ISteamService
    {
        Task<IEnumerable<ListingProxy>> GetFilterredListingsAsync(short page);

        Task<IEnumerable<ListingProxy>> GetPaintedListingsOnlyAsync(short page);

        Task<(bool,string)> IsListingPaintedAsync(string name);

        Task<IEnumerable<SellListing>> GetSellListingsAsync();
    }

}
