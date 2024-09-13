using Microsoft.EntityFrameworkCore;
using SteamAppServer.Context;
using SteamAppServer.Models;
using SteamAppServer.Repositories.Interfaces;

namespace SteamAppServer.Repositories
{
    public class SteamRepository : ISteamRepository
    {
        private readonly ApplicationDbContext _context;

        public SteamRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SellListing>> GetSellListingsAsync()
        {
            var result = await _context.SellListings.ToListAsync();
            return result;
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
