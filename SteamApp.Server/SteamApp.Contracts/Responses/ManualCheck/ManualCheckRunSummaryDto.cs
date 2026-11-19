using SteamApp.Contracts.Common;
using SteamApp.Contracts.Enums;

namespace SteamApp.Contracts.ManualCheck;

public class ManualCheckRunSummaryDto
{
    public long Id { get; set; }
    public long? PresetId { get; set; }
    public string PresetName { get; set; } = string.Empty;
    public long GameId { get; set; }
    public string? GameName { get; set; }
    public long GameUrlId { get; set; }
    public string? GameUrlName { get; set; }
    public int TotalProducts { get; set; }
    public int CheckedProducts { get; set; }
    public int MatchedProducts { get; set; }
    public int FailedProducts { get; set; }
    public ManualCheckProgressDto Progress { get; set; } = new();

    [StringEncodedEnum]
    public ManualCheckRunStatusEnum Status { get; set; }

    [StringEncodedEnum]
    public AutomationPauseReasonEnum? PauseReason { get; set; }

    public DateTime Date { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public long? DurationMilliseconds { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string? ErrorText { get; set; }
    public long? AutomaticQueueRunId { get; set; }
    public string? AutomaticQueueName { get; set; }
    public int? AutomaticQueueBlockIndex { get; set; }
}
