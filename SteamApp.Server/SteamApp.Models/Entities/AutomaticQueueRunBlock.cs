using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SteamApp.Domain.Enums;

namespace SteamApp.Domain.Entities;

[Table("automatic_queue_run_block")]
public sealed class AutomaticQueueRunBlock
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("automatic_queue_run_id")]
    public long AutomaticQueueRunId { get; set; }

    public AutomaticQueueRun AutomaticQueueRun { get; set; } = null!;

    [Column("block_key")]
    public Guid BlockKey { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [Column("block_type")]
    public AutomaticQueueBlockTypeEnum BlockType { get; set; }

    [Column("status")]
    public AutomaticQueueBlockRunStatusEnum Status { get; set; } = AutomaticQueueBlockRunStatusEnum.Pending;

    [Required]
    [Column("setup_json")]
    public string SetupJson { get; set; } = "{}";

    [Column("manual_check_run_id")]
    public long? ManualCheckRunId { get; set; }

    public ManualCheckRun? ManualCheckRun { get; set; }

    [Column("wait_until_utc")]
    public DateTime? WaitUntilUtc { get; set; }

    [Column("remaining_delay_seconds")]
    public int? RemainingDelaySeconds { get; set; }

    [Column("started_at_utc")]
    public DateTime? StartedAtUtc { get; set; }

    [Column("completed_at_utc")]
    public DateTime? CompletedAtUtc { get; set; }

    [Column("error_text")]
    public string? ErrorText { get; set; }
}
