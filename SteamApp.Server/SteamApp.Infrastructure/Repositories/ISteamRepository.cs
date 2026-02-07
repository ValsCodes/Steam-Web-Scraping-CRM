using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Repositories
{
    public interface ISteamRepository
    {
        Task<GameUrl> GetGameUrl(long gameUrlId);

        Task<Game> GetGame(long gameId);
    }
}
