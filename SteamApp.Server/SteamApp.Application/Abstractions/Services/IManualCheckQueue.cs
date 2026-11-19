using SteamApp.Contracts.ManualCheck;
using SteamApp.Application.DTOs.ManualCheck;

namespace SteamApp.Application.Abstractions.Services;

public interface IManualCheckQueue
{
    ValueTask EnqueueAsync(long runId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<ManualCheckQueueItem> ReadAllAsync(CancellationToken cancellationToken);
    bool TryPause(long runId);
    bool TryCancel(long runId);
    void Complete(long runId, Guid workItemId);
}
