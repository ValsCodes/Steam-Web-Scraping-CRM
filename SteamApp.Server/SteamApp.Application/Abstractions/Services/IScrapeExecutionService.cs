using SteamApp.Contracts.WatchItem;

namespace SteamApp.Application.Abstractions.Services;

public interface IScrapeExecutionService
{
    Task<IReadOnlyList<WatchItemDto>> ExecuteAsync(
        string endpoint,
        long gameUrlId,
        short page);
}
