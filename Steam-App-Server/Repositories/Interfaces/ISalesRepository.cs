using SteamAppServer.Models;
using SteamAppServer.Models.Partials;

namespace SteamAppServer.Repositories.Interfaces
{
    public interface ISalesRepository
    {
        Task<SellListing?> CreateListingAsync(SellListing sellListing);
        Task<IEnumerable<SellListing>> GetListingsAsync();
        Task<SellListing?> UpdateListingAsync(long id, SellListing sellListing);
        Task<SellListing?> UpdateListingPartialAsync(long id, SellListingPartial sellListing);
        Task<SellListing?> DeleteListingAsync(long id);

    }
}
