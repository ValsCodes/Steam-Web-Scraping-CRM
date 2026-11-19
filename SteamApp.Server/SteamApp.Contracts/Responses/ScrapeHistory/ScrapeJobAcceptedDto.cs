using SteamApp.Contracts.Common;
using SteamApp.Contracts.Enums;

namespace SteamApp.Contracts.ScrapeHistory;

public sealed class ScrapeJobAcceptedDto
{
    public long HistoryId { get; set; }
    public ScrapeHistorySummaryDto History { get; set; } = new();
    [StringEncodedEnum]
    public ScrapeJobStatusEnum Status { get; set; } = ScrapeJobStatusEnum.Queued;
    public string CorrelationId { get; set; } = string.Empty;
}
