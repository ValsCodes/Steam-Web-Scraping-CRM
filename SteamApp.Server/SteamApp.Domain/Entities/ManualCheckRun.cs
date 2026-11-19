using System.ComponentModel.DataAnnotations;
using SteamApp.Domain.Enums;

namespace SteamApp.Domain.Entities;

public sealed class ManualCheckRun
{
    public long Id { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    public long? ManualCheckPresetId { get; set; }

    public ManualCheckPreset? ManualCheckPreset { get; set; }

    public long GameId { get; set; }

    public Game Game { get; set; } = null!;

    public long GameUrlId { get; set; }

    public GameUrl GameUrl { get; set; } = null!;

    [Required]
    [MaxLength(ManualCheckPreset.NameMaxLength)]
    public string PresetName { get; set; } = string.Empty;

    [Required]
    public string SetupJson { get; set; } = "{}";

    public string? ResultsJson { get; set; }

    public int TotalProducts { get; set; }

    public int CheckedProducts { get; set; }

    public int MatchedProducts { get; set; }

    public int FailedProducts { get; set; }

    public ManualCheckRunStatusEnum Status { get; set; } = ManualCheckRunStatusEnum.Queued;

    public AutomationPauseReasonEnum? PauseReason { get; set; }

    public DateTime Date { get; set; }

    public DateTime? StartedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public DateTime? PausedAtUtc { get; set; }

    public long AccumulatedPausedMilliseconds { get; set; }

    public string? ErrorText { get; set; }

    [MaxLength(64)]
    public string CorrelationId { get; set; } = string.Empty;

    public AutomaticQueueRunBlock? AutomaticQueueRunBlock { get; set; }

    public ICollection<AutomationUsageInterval> UsageIntervals { get; set; } = [];
}
