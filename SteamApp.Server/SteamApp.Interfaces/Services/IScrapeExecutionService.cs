using SteamApp.Application.DTOs.WatchItem;

namespace SteamApp.Interfaces.Services;

public interface IScrapeExecutionService
{
    Task<IReadOnlyList<WatchItemDto>> ExecuteAsync(
        string endpoint,
        long gameUrlId,
        short page);
}
