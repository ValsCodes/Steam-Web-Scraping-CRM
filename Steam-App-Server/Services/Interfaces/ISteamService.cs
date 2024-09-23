using SteamAppServer.Models;
using SteamAppServer.Models.Proxies;

namespace SteamAppServer.Services.Interfaces
{
    public interface ISteamService
    {
        Task<IEnumerable<ListingProxy>> GetFilterredListingsAsync(short page);
        Task<IEnumerable<ListingProxy>> GetPaintedListingsOnlyAsync(short page);
        Task<(bool, string)> IsListingPaintedAsync(string name);
        Task<SellListing?> CreateListingAsync(SellListing sellListing);
        Task<IEnumerable<SellListing>> GetListingsAsync();
        Task<SellListing?> UpdateListingAsync(long id, SellListing sellListing);
        Task<SellListing?> DeleteListingAsync(long id);
    }
}
