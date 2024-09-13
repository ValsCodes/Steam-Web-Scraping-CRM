using SteamAppServer.Context;
using SteamAppServer.Models;

namespace SteamAppServer.Repositories
{
    public class SteamRepository
    {
        private readonly ApplicationDbContext _context;

        public SteamRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void GetSellListings()
        {

        }

        public void AddListing()
        {

        }

        public void DeleteListing(long id)
        {

        }

        public void UpdateListing(long id, SellListing sellListing) 
        {

        }
    }
}
