using SteamApp.Contracts.Common;
using SteamApp.Contracts.Enums;

namespace SteamApp.Contracts.AutomaticQueue;

public sealed class AutomaticQueueRunBlockDto
{
    public long Id { get; set; }
    public Guid Key { get; set; }
    public int SortOrder { get; set; }

    [StringEncodedEnum]
    public AutomaticQueueBlockTypeEnum Type { get; set; }

    [StringEncodedEnum]
    public AutomaticQueueBlockRunStatusEnum Status { get; set; }

    public AutomaticQueueBlockDto Configuration { get; set; } = new();
    public long? ManualCheckRunId { get; set; }
    public int WarningCount { get; set; }
    public DateTime? WaitUntilUtc { get; set; }
    public int? RemainingDelaySeconds { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public string? ErrorText { get; set; }
}
