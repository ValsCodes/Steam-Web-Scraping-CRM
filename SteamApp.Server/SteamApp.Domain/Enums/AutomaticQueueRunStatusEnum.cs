namespace SteamApp.Domain.Enums;

public enum AutomaticQueueRunStatusEnum
{
    Queued = 1,
    Running = 2,
    PauseRequested = 3,
    Paused = 4,
    Succeeded = 5,
    CompletedWithErrors = 6,
    Failed = 7,
    Canceled = 8
}
