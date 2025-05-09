using Microsoft.EntityFrameworkCore;
using SteamApp.Context;
using SteamApp.Infrastructure.Repositories;
using SteamApp.Infrastructure.DTOs;
using SteamApp.Infrastructure.Models;
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

        public async Task<IEnumerable<IItem>> GetItemsAsync()
        {
            return await _context.Items.OrderBy(x => x.Id).Select(x => new ItemDto {
                             Id = x.Id,
                             Name = x.Name,
                             IsActive = x.IsActive,
                             IsWeapon = x.IsWeapon
                         })
                         .ToListAsync<IItem>();
        }

        public async Task<IItem> GetItemByIdAsync(long id)
        {
            var result = await _context.Items.FirstOrDefaultAsync(x => x.Id == id);              
            
            if (result == null)
            {
                throw new Exception("Item not found");
            }

            return (IItem)result;
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
