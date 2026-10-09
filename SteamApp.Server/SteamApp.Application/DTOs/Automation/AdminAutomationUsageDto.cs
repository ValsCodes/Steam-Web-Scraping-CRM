namespace SteamApp.Application.DTOs.Automation;

public sealed class AdminAutomationUsageDto
{
    public string UserId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsAdmin { get; set; }
    public AutomationUsageDto Usage { get; set; } = new();
}
