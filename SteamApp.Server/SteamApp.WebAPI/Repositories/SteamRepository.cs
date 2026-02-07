using Microsoft.EntityFrameworkCore;
using SteamApp.Domain.Entities;
using SteamApp.Infrastructure.Repositories;
using SteamApp.WebAPI.Context;

namespace SteamApp.WebAPI.Repositories;

public class SteamRepository(ApplicationDbContext dbContext) : ISteamRepository
{
    public async Task<GameUrl> GetGameUrl(long gameUrlId)
    {
        var gameUrl = await dbContext.GameUrls
            .AsNoTracking()
            .Include(x => x.Game)
            .Include(x => x.GameUrlsPixels)
                .ThenInclude(gup => gup.Pixel)
            .Include(x => x.GameUrlsProducts)
            .FirstOrDefaultAsync(g => g.Id == gameUrlId);

        if (gameUrl is null)
        {
            throw new Exception($"GameUrl with id {gameUrlId} not found.");

        }

        return gameUrl;
    }

    public async Task<Game> GetGame(long gameId)
    {
        var game = await dbContext.Games.AsNoTracking()
            .Include(x => x.Pixels)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game is null)
        {
            throw new Exception($"Game with id {game} not found.");

        }

        return game;
    }
}
