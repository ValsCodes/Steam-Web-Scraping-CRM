using SteamApp.Domain.Entities;

namespace SteamApp.Application.Abstractions.Persistence
{
    public interface ISteamRepository
    {
        Task<GameUrl> GetGameUrl(long gameUrlId);

        Task<Game> GetGame(long gameId);
    }
}
