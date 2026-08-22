namespace SteamApp.Application.DTOs.ManualCheck;

public readonly record struct ManualCheckQueueItem(long RunId, CancellationToken CancellationToken);
