using Newtonsoft.Json.Converters;
using SteamApp.Domain.Enums;

namespace SteamApp.Application.DTOs.ScrapeHistory;

public sealed class ScrapeJobAcceptedDto
{
    public long HistoryId { get; set; }
    public ScrapeHistorySummaryDto History { get; set; } = new();
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public ScrapeJobStatusEnum Status { get; set; } = ScrapeJobStatusEnum.Queued;
    public string CorrelationId { get; set; } = string.Empty;
}
