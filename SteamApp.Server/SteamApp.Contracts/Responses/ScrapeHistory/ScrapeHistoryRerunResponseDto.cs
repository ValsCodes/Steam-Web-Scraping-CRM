using SteamApp.Contracts.WatchItem;

namespace SteamApp.Contracts.ScrapeHistory;

public sealed class ScrapeHistoryRerunResponseDto
{
    public ScrapeHistorySummaryDto History { get; set; } = new();
    public IEnumerable<WatchItemDto> Results { get; set; } = [];
    public string? ErrorText { get; set; }
}
