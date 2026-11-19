namespace SteamApp.Domain.Enums;

public enum ManualCheckRunStatusEnum
{
    Queued = 1,
    Running = 2,
    Succeeded = 3,
    CompletedWithErrors = 4,
    Failed = 5,
    Canceled = 6,
    PauseRequested = 7,
    Paused = 8
}
