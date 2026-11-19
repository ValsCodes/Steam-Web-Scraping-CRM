namespace SteamApp.Contracts.ManualCheck;

public sealed class ManualCheckRunDetailDto : ManualCheckRunSummaryDto
{
    public ManualCheckSetupDto Setup { get; set; } = new();
    public ManualCheckRunResultsDto Results { get; set; } = new();
}
