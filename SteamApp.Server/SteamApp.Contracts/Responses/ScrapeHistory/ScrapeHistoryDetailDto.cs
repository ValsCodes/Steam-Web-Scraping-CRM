using SteamApp.Contracts.Common;
using SteamApp.Contracts.Enums;

namespace SteamApp.Contracts.ScrapeHistory;

public sealed class ScrapeHistoryDetailDto
{
    public long Id { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string ScrapeType { get; set; } = string.Empty;
    public long GameUrlId { get; set; }
    public string? GameUrlName { get; set; }
    public short Page { get; set; }
    public int ResultCount { get; set; }
    public DateTime Date { get; set; }
    public bool IsHaveError { get; set; }
    [StringEncodedEnum]
    public ScrapeJobStatusEnum Status { get; set; } = ScrapeJobStatusEnum.Succeeded;
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public string? CorrelationId { get; set; }
    public string SetupJson { get; set; } = "{}";
    public string? ResultsJson { get; set; }
    public string? ErrorText { get; set; }
}
