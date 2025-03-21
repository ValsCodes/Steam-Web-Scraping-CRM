using Microsoft.EntityFrameworkCore;
using SteamApp.Context;
using SteamApp.Infrastructure.Repositories;
using SteamApp.Models.DTOs;
using SteamApp.Models.Models;

namespace SteamApp.Repository
{
    public class ItemRepository : IItemRepository
    {
        private readonly ApplicationDbContext _context;

        public ItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Item>> GetItemsAsync()
        {
            return _context.Items.OrderBy(x => x.Id);
        }

        public async Task<Item> GetItemByIdAsync(long id)
        {
            var result = await _context.Items.FirstOrDefaultAsync(x => x.Id == id);

            if (result == null)
            {
                throw new Exception("Item not found");
            }

            return result;
        }

        public async Task<bool> CreateItemAsync(ItemDto item)
        {
            

            throw new NotImplementedException();
        }

        public async Task<bool> DeleteItemAsync(long id)
        {
            throw new NotImplementedException();
        }

        public async Task<Item> GetItemAsync(long id)
        {
            throw new NotImplementedException();
        }
    }
}
