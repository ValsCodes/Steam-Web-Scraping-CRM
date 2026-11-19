using System.ComponentModel.DataAnnotations;
using SteamApp.Domain.Enums;

namespace SteamApp.Domain.Entities;

public sealed class AutomaticQueueRun
{
    public long Id { get; set; }

    public long? AutomaticQueueDefinitionId { get; set; }

    public AutomaticQueueDefinition? AutomaticQueueDefinition { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(AutomaticQueueDefinition.NameMaxLength)]
    public string QueueName { get; set; } = string.Empty;

    public AutomaticQueueRunStatusEnum Status { get; set; } = AutomaticQueueRunStatusEnum.Queued;

    public AutomationPauseReasonEnum? PauseReason { get; set; }

    public int CurrentBlockIndex { get; set; }

    public DateTime Date { get; set; }

    public DateTime? StartedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public string? ErrorText { get; set; }

    [Required]
    [MaxLength(64)]
    public string CorrelationId { get; set; } = string.Empty;

    public ICollection<AutomaticQueueRunBlock> Blocks { get; set; } = [];
}
