using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SteamApp.Application.Caching;
using SteamApp.Domain.Entities;
using SteamApp.Infrastructure.Context;
using SteamApp.Interfaces.Repositories;

namespace SteamApp.Infrastructure.Repositories;

public class WishlistRepository(
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    IMemoryCache cache) : IWishlistRepository
{
    public async Task<WishList?> GetAsync(long id, CancellationToken ct)
    {
        var cacheKey = string.Format(CacheKeys.WishListItem, id);

        if (cache.TryGetValue<WishList>(cacheKey, out var cached) && cached is not null)
        {
            return cached;
        }

        await using var dbContext = dbContextFactory.CreateDbContext();

        var wishList = await dbContext.WishLists.AsNoTracking()
            .Include(x => x.Game)
            .FirstOrDefaultAsync(g => g.Id == id, ct);

        if (wishList is null)
        {
            throw new Exception($"WishList with id {id} not found.");
        }

        cache.Set(cacheKey, wishList, TimeSpan.FromMinutes(1));

        return wishList;
    }

    public async Task<IEnumerable<WishList>> GetAllAsync(CancellationToken ct)
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var entities = await dbContext.WishLists.AsNoTracking().ToListAsync(ct);

        return entities;
    }
}
