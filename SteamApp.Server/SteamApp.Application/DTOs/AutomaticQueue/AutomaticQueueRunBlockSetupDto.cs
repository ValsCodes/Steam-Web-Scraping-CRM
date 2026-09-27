using SteamApp.Application.DTOs.ManualCheck;

namespace SteamApp.Application.DTOs.AutomaticQueue;

public sealed class AutomaticQueueRunBlockSetupDto
{
    public AutomaticQueueBlockDto Configuration { get; set; } = new();
    public ManualCheckSetupDto? ManualCheckSetup { get; set; }
    public int? DelaySeconds { get; set; }
}
