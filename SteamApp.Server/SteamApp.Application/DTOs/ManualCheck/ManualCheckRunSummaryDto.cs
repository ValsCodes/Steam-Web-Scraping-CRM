using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SteamApp.Domain.Enums;

namespace SteamApp.Application.DTOs.ManualCheck;

public class ManualCheckRunSummaryDto
{
    public long Id { get; set; }
    public long? PresetId { get; set; }
    public string PresetName { get; set; } = string.Empty;
    public long GameId { get; set; }
    public string? GameName { get; set; }
    public long GameUrlId { get; set; }
    public string? GameUrlName { get; set; }
    public int TotalProducts { get; set; }
    public int CheckedProducts { get; set; }
    public int MatchedProducts { get; set; }
    public int FailedProducts { get; set; }
    public ManualCheckProgressDto Progress { get; set; } = new();

    [JsonConverter(typeof(StringEnumConverter))]
    public ManualCheckRunStatusEnum Status { get; set; }

    public DateTime Date { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public long? DurationMilliseconds { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string? ErrorText { get; set; }
}
