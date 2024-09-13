using SteamAppServer.Models;

namespace SteamAppServer.Repositories.Interfaces
{
    public interface ISteamRepository
    {
        Task<IEnumerable<SellListing>> GetSellListingsAsync();
    }
}
