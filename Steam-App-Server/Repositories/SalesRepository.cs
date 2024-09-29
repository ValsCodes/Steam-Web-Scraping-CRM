using Microsoft.EntityFrameworkCore;
using SteamAppServer.Context;
using SteamAppServer.Models;
using SteamAppServer.Models.Partials;
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
            existingListing.QualityId = sellListing.QualityId;
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

        public async Task<SellListing?> UpdateListingPartialAsync(long id, SellListingPartial sellListing)
        {
            var existingListing = await _context.SellListings.FindAsync(id);

            if (existingListing == null)
            {
                return null;
            }

            if (sellListing.Name != null) existingListing.Name = sellListing.Name;
            if (sellListing.QualityId != null) existingListing.QualityId = sellListing.QualityId;
            if (sellListing.Description != null) existingListing.Description = sellListing.Description;
            if (sellListing.DateBought != null) existingListing.DateBought = sellListing.DateBought.Value;
            if (sellListing.DateSold != null) existingListing.DateSold = sellListing.DateSold;
            if (sellListing.CostPrice.HasValue) existingListing.CostPrice = sellListing.CostPrice.Value;
            if (sellListing.TargetSellPrice1.HasValue) existingListing.TargetSellPrice1 = sellListing.TargetSellPrice1.Value;
            if (sellListing.TargetSellPrice2.HasValue) existingListing.TargetSellPrice2 = sellListing.TargetSellPrice2.Value;
            if (sellListing.TargetSellPrice3.HasValue) existingListing.TargetSellPrice3 = sellListing.TargetSellPrice3.Value;
            if (sellListing.TargetSellPrice4.HasValue) existingListing.TargetSellPrice4 = sellListing.TargetSellPrice4.Value;
            if (sellListing.SoldPrice.HasValue) existingListing.SoldPrice = sellListing.SoldPrice.Value;
            if (sellListing.IsHat.HasValue) existingListing.IsHat = sellListing.IsHat.Value;
            if (sellListing.IsWeapon.HasValue) existingListing.IsWeapon = sellListing.IsWeapon.Value;
            if (sellListing.IsSold.HasValue) existingListing.IsSold = sellListing.IsSold.Value;

            await _context.SaveChangesAsync();

            return existingListing; 
        }
    }
}
