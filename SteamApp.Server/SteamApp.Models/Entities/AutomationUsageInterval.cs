using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Domain.Entities;

[Table("automation_usage_interval")]
public sealed class AutomationUsageInterval
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [MaxLength(450)]
    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;

    [Column("manual_check_run_id")]
    public long ManualCheckRunId { get; set; }

    public ManualCheckRun ManualCheckRun { get; set; } = null!;

    [Column("started_at_utc")]
    public DateTime StartedAtUtc { get; set; }

    [Column("ended_at_utc")]
    public DateTime? EndedAtUtc { get; set; }
}
