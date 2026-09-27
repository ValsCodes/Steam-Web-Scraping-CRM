namespace SteamApp.Domain.Enums;

public enum AutomaticQueueBlockRunStatusEnum
{
    Pending = 1,
    Running = 2,
    Paused = 3,
    Succeeded = 4,
    CompletedWithWarnings = 5,
    Failed = 6,
    Canceled = 7,
    Skipped = 8
}
