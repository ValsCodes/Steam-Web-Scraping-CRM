using Microsoft.EntityFrameworkCore;
using SteamAppServer.Context;
using SteamAppServer.Models;
using SteamAppServer.Repositories.Interfaces;

namespace SteamAppServer.Repositories
{
    public class SalesRepository : ISalesRepository
    {
        private readonly ApplicationDbContext _context;

        public SalesRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SellListing>> GetListingsAsync()
        {
            var result = await _context.SellListings.ToListAsync();
            return result;
        }

        public async Task<SellListing?> CreateListingAsync(SellListing sellListing)
        {
            await _context.AddAsync(sellListing);
            await _context.SaveChangesAsync();

            return sellListing;
        }

        public async Task<SellListing?> DeleteListingAsync(long id)
        {
            var existingListing = await _context.SellListings.FindAsync(id);

            if (existingListing == null)
            {
                return null;
            }

            _context.Remove(existingListing);
            await _context.SaveChangesAsync();

            return existingListing;
        }

        public async Task<SellListing?> UpdateListingAsync(long id, SellListing sellListing)
        {
            var existingListing = await _context.SellListings.FindAsync(id);

            if (existingListing == null)
            {
                return null;
            }

            existingListing.Name = sellListing.Name;
            existingListing.Description = sellListing.Description;
            existingListing.DateBought = sellListing.DateBought;
            existingListing.DateSold = sellListing.DateSold;
            existingListing.CostPrice = sellListing.CostPrice;
            existingListing.TargetSellPrice1 = sellListing.TargetSellPrice1;
            existingListing.TargetSellPrice2 = sellListing.TargetSellPrice2;
            existingListing.TargetSellPrice3 = sellListing.TargetSellPrice3;
            existingListing.TargetSellPrice4 = sellListing.TargetSellPrice4;
            existingListing.SoldPrice = sellListing.SoldPrice;
            existingListing.IsHat = sellListing.IsHat;
            existingListing.IsWeapon = sellListing.IsWeapon;
            existingListing.IsSold = sellListing.IsSold;

            await _context.SaveChangesAsync();

            return existingListing;
        }
    }
}
