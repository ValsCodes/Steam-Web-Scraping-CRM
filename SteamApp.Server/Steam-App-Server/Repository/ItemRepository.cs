using Microsoft.EntityFrameworkCore;
using SteamApp.Context;
using SteamApp.Exceptions;
using SteamApp.Infrastructure.Repositories;
using SteamApp.Models.Models;

namespace SteamApp.Repository
{
    public class ItemRepository(ApplicationDbContext context) : IItemRepository
    {
        public async Task<IEnumerable<Item>> GetItemsAsync(CancellationToken ct, IEnumerable<long>? classFilters = null, IEnumerable<long>? slotFilters = null)
        {
            IQueryable<Item> items = context.Items;

            if (classFilters?.Any() == true)
            {
                var arr = classFilters.ToArray();
                items = items.Where(x => x.ClassId.HasValue && arr.Contains(x.ClassId.Value));
            }

            if (slotFilters?.Any() == true)
            {
                var arr = slotFilters.ToArray();
                items = items.Where(x => x.SlotId.HasValue && arr.Contains(x.SlotId.Value));
            }

            return await items.ToArrayAsync(ct);
        }

        public async Task<Item> GetItemByIdAsync(long id, CancellationToken ct)
        {
            var result = await context.Items.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);              
            
            if (result == null)
            {
                throw new Exception("Item not found");
            }

            return result;
        }

        public async Task<long> CreateItemAsync(Item item, CancellationToken ct)
        {
            await context.AddAsync(item, ct);
            await context.SaveChangesAsync(ct);

            return item.Id;
        }

        public async Task<bool> UpdateItemAsync(Item item, CancellationToken ct)
        {
            context.Items.Update(item);
            await context.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> DeleteItemAsync(long id, CancellationToken ct)
        {
            var existingItem = await context.Items.FindAsync(id, ct);

            if (existingItem == null)
            {
                throw new ItemNotFoundException($"Item with ID {id} not found.");
            }

            context.Remove(existingItem);
            await context.SaveChangesAsync(ct);

            return true;
        }
    }
}
