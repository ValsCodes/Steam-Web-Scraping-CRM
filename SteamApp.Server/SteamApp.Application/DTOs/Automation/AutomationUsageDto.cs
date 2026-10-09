namespace SteamApp.Application.DTOs.Automation;

public sealed class AutomationUsageDto
{
    public bool Unlimited { get; set; }
    public int? LimitSeconds { get; set; }
    public int UsedSeconds { get; set; }
    public int? RemainingSeconds { get; set; }
    public DateTime? UsageResetAtUtc { get; set; }
    public bool PresenceRequired { get; set; }
    public bool PresenceActive { get; set; }
    public DateTime? PresenceExpiresAtUtc { get; set; }
    public DateTime? NextAllowanceAtUtc { get; set; }
}
