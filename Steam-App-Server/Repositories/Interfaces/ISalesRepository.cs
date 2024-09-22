using SteamAppServer.Models;

namespace SteamAppServer.Repositories.Interfaces
{
    public interface ISalesRepository
    {
        Task<SellListing?> CreateListingAsync(SellListing sellListing);
        Task<IEnumerable<SellListing>> GetListingsAsync();
        Task<SellListing?> UpdateListingAsync(long id, SellListing sellListing);
        Task<SellListing?> DeleteListingAsync(long id);

    }
}
