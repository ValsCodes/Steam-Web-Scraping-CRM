namespace SteamApp.Contracts.ManualCheck;

public sealed class ManualCheckRunAcceptedDto
{
    public long RunId { get; set; }
    public ManualCheckRunSummaryDto Run { get; set; } = null!;
}
