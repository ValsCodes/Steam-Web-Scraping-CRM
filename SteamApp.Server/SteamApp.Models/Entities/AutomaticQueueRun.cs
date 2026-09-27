using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SteamApp.Domain.Enums;

namespace SteamApp.Domain.Entities;

[Table("automatic_queue_run")]
public sealed class AutomaticQueueRun
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("automatic_queue_definition_id")]
    public long? AutomaticQueueDefinitionId { get; set; }

    public AutomaticQueueDefinition? AutomaticQueueDefinition { get; set; }

    [Required]
    [MaxLength(450)]
    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(AutomaticQueueDefinition.NameMaxLength)]
    [Column("queue_name")]
    public string QueueName { get; set; } = string.Empty;

    [Column("status")]
    public AutomaticQueueRunStatusEnum Status { get; set; } = AutomaticQueueRunStatusEnum.Queued;

    [Column("current_block_index")]
    public int CurrentBlockIndex { get; set; }

    [Column("date")]
    public DateTime Date { get; set; }

    [Column("started_at_utc")]
    public DateTime? StartedAtUtc { get; set; }

    [Column("completed_at_utc")]
    public DateTime? CompletedAtUtc { get; set; }

    [Column("error_text")]
    public string? ErrorText { get; set; }

    [Required]
    [MaxLength(64)]
    [Column("correlation_id")]
    public string CorrelationId { get; set; } = string.Empty;

    public ICollection<AutomaticQueueRunBlock> Blocks { get; set; } = [];
}
