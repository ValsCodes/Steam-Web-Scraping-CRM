using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SteamApp.Application.Caching;
using SteamApp.Application.Repositories;
using SteamApp.Domain.Entities;
using SteamApp.Infrastructure.Context;

namespace SteamApp.Infrastructure.Repositories;

public class WishlistRepository(ApplicationDbContext dbContext, IMemoryCache cache) : IWishlistRepository
{
    public async Task<WishList?> GetAsync(long id, CancellationToken ct)
    {
        var cacheKey = string.Format(CacheKeys.WishListItem, id);

        if (cache.TryGetValue(cacheKey, out object cached))
        {
            return (WishList)cached;
        }

        var wishList = await dbContext.WishLists.AsNoTracking()
            .Include(x => x.Game)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (wishList is null)
        {
            throw new Exception($"WishList with id {wishList} not found.");
        }

        cache.Set(cacheKey, wishList, TimeSpan.FromMinutes(1));

        return wishList;
    }

    public async Task<IEnumerable<WishList>> GetAllAsync(CancellationToken ct)
    {
        var entities = await dbContext.WishLists.AsNoTracking().ToListAsync(ct);

        return entities;
    }
}
