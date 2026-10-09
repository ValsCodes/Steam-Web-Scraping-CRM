namespace SteamApp.Application.DTOs.Automation;

public sealed class AutomationPolicyUpdateDto
{
    public int NonAdminLimitMinutes { get; set; }
    public string RowVersion { get; set; } = string.Empty;
}
