using SteamApp.Domain.Entities;

namespace SteamApp.Interfaces.Repositories
{
    public interface ISteamRepository
    {
        Task<GameUrl> GetGameUrl(long gameUrlId);

        Task<Game> GetGame(long gameId);
    }
}
