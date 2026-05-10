using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SteamApp.Application.Caching;
using SteamApp.Domain.Entities;
using SteamApp.Infrastructure.Context;
using SteamApp.Interfaces.Repositories;


namespace SteamApp.Infrastructure.Repositories;

public class SteamRepository(ApplicationDbContext dbContext, IMemoryCache cache) : ISteamRepository
{
    public async Task<GameUrl> GetGameUrl(long gameUrlId)
    {

        var cacheKey = string.Format(CacheKeys.GameUrl, gameUrlId);

        if (cache.TryGetValue<GameUrl>(cacheKey, out var cached) && cached is not null)
        {
            return cached;
        }

        var gameUrl = await dbContext.GameUrls
            .AsNoTracking()
            .Include(x => x.Game)
            .Include(x => x.GameUrlsPixels)
                .ThenInclude(gup => gup.Pixel)
            .Include(x => x.GameUrlsProducts)
                .ThenInclude(gup => gup.Product)
            .FirstOrDefaultAsync(g => g.Id == gameUrlId);

        if (gameUrl is null)
        {
            throw new Exception($"GameUrl with id {gameUrlId} not found.");

        }

        cache.Set(cacheKey, gameUrl, TimeSpan.FromMinutes(1));
        return gameUrl;
    }

    public async Task<Game> GetGame(long gameId)
    {
        var cacheKey = string.Format(CacheKeys.Game, gameId);

        if (cache.TryGetValue<Game>(cacheKey, out var cached) && cached is not null)
        {
            return cached;
        }

        var game = await dbContext.Games.AsNoTracking()
            .Include(x => x.Pixels)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game is null)
        {
            throw new Exception($"Game with id {gameId} not found.");

        }

        cache.Set(cacheKey, game, TimeSpan.FromMinutes(1));

        return game;
    }
}
