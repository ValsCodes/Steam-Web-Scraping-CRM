using Microsoft.EntityFrameworkCore;
using SteamApp.Infrastructure.Repositories;
using SteamApp.Models.Entities;
using SteamApp.WebAPI.Context;
using SteamApp.WebAPI.Exceptions;

namespace SteamApp.WebAPI.Repositories
{
    public class ItemRepository(ApplicationDbContext context) : IItemRepository
    {
        public async Task<IEnumerable<Item>> GetItems(CancellationToken ct, string? name = null, IEnumerable<long>? classFilters = null, IEnumerable<long>? slotFilters = null)
        {
            IQueryable<Item> items = context.Item;

            if (!string.IsNullOrEmpty(name))
            {
                var formattedName = name.Trim().ToLower();
                items = items.Where(x => x.Name.ToLower().Trim().StartsWith(name) || x.Name.ToLower().Trim().Contains(name));
            }

            return await items.OrderBy(x => x.Id).ToArrayAsync(ct);
        }

        public async Task<Item> GetItemById(long id, CancellationToken ct)
        {
            var result = await context.Item.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);              
        
            if (result == null)
            {
                throw new Exception("Item not found");
            }

            return result;
        }

        public async Task<IEnumerable<Item>> GetItemByNameAsync(string name, CancellationToken ct)
        {
            var formattedName = name.Trim().ToLower();
            var result = await context.Item.AsNoTracking().Where(x => x.Name.Trim().ToLower().Contains(formattedName)).ToArrayAsync(ct);

            if (result == null)
            {
                throw new Exception("Item not found");
            }

            return result;
        }

        public async Task<long> CreateItem(Item item, CancellationToken ct)
        {
            await context.AddAsync(item, ct);
            await context.SaveChangesAsync(ct);

            return item.Id;
        }

        public async Task<bool> UpdateItem(Item item, CancellationToken ct)
        {
            context.Item.Update(item);
            await context.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> DeleteItem(long id, CancellationToken ct)
        {
            var existingItem = await context.Item.FindAsync(id, ct);

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
