using System.ComponentModel.DataAnnotations;
using SteamApp.Domain.Enums;

namespace SteamApp.Domain.Entities;

public sealed class AutomaticQueueRunBlock
{
    public long Id { get; set; }

    public long AutomaticQueueRunId { get; set; }

    public AutomaticQueueRun AutomaticQueueRun { get; set; } = null!;

    public Guid BlockKey { get; set; }

    public int SortOrder { get; set; }

    public AutomaticQueueBlockTypeEnum BlockType { get; set; }

    public AutomaticQueueBlockRunStatusEnum Status { get; set; } = AutomaticQueueBlockRunStatusEnum.Pending;

    [Required]
    public string SetupJson { get; set; } = "{}";

    public long? ManualCheckRunId { get; set; }

    public ManualCheckRun? ManualCheckRun { get; set; }

    public DateTime? WaitUntilUtc { get; set; }

    public int? RemainingDelaySeconds { get; set; }

    public DateTime? StartedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public string? ErrorText { get; set; }
}
