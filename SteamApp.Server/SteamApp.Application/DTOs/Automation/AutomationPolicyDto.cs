namespace SteamApp.Application.DTOs.Automation;

public sealed class AutomationPolicyDto
{
    public int NonAdminLimitMinutes { get; set; }
    public DateTime? UsageResetAtUtc { get; set; }
    public string? LastModifiedByUserId { get; set; }
    public DateTime LastModifiedAtUtc { get; set; }
    public string? LastResetByUserId { get; set; }
    public DateTime? LastResetAtUtc { get; set; }
    public string RowVersion { get; set; } = string.Empty;
}
