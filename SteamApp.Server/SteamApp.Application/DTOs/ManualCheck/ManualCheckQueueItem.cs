namespace SteamApp.Application.DTOs.ManualCheck;

public readonly record struct ManualCheckQueueItem(
    long RunId,
    Guid WorkItemId,
    CancellationToken CancellationToken,
    CancellationToken PauseToken);
