namespace SteamApp.Application.DTOs.ManualCheck;

public sealed class ManualCheckRunDetailDto : ManualCheckRunSummaryDto
{
    public ManualCheckSetupDto Setup { get; set; } = new();
    public ManualCheckRunResultsDto Results { get; set; } = new();
}
