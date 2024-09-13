using SteamAppServer.Models;

namespace SteamAppServer.Repositories.Interfaces
{
    public interface ISteamRepository
    {
        Task<IEnumerable<SellListing>> GetSellListingsAsync();

        Task<SellListing?> AddListingAsync(SellListing sellListing);

        Task<SellListing?> DeleteListingAsync(long id);

        Task<SellListing?> UpdateListingAsync(long id, SellListing sellListing);
    }
}
