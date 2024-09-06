using SteamAppServer.Models.Proxies;

namespace SteamAppServer.Services.Interfaces
{
    public interface ISteamService
    {
        public Task<IEnumerable<ListingProxy>> GetFilterredListingsAsync(short page);

        public Task<IEnumerable<ListingProxy>> GetPaintedListingsOnlyAsync(short page);

        public Task<(bool,string)> IsListingPaintedAsync(string name);
    }
}
