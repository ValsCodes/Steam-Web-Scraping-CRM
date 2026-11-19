using System.ComponentModel.DataAnnotations;

namespace SteamApp.Domain.Entities;

public sealed class AutomationUsageInterval
{
    public long Id { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    public long ManualCheckRunId { get; set; }

    public ManualCheckRun ManualCheckRun { get; set; } = null!;

    public DateTime StartedAtUtc { get; set; }

    public DateTime? EndedAtUtc { get; set; }
}
