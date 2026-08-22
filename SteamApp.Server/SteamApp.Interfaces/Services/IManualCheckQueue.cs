using SteamApp.Application.DTOs.ManualCheck;

namespace SteamApp.Interfaces.Services;

public interface IManualCheckQueue
{
    ValueTask EnqueueAsync(long runId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<ManualCheckQueueItem> ReadAllAsync(CancellationToken cancellationToken);
    bool TryCancel(long runId);
    void Complete(long runId);
}
