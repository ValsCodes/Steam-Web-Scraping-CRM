using SteamApp.Application.DTOs.WatchItem;

namespace SteamApp.Application.DTOs.ScrapeHistory;

public sealed class ScrapeHistoryRerunResponseDto
{
    public ScrapeHistorySummaryDto History { get; set; } = new();
    public IEnumerable<WatchItemDto> Results { get; set; } = [];
    public string? ErrorText { get; set; }
}
