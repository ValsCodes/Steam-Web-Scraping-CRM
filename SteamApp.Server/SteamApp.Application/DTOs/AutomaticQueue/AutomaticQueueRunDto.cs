using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SteamApp.Domain.Enums;

namespace SteamApp.Application.DTOs.AutomaticQueue;

public sealed class AutomaticQueueRunDto
{
    public long Id { get; set; }
    public long? QueueId { get; set; }
    public string QueueName { get; set; } = string.Empty;

    [JsonConverter(typeof(StringEnumConverter))]
    public AutomaticQueueRunStatusEnum Status { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public AutomationPauseReasonEnum? PauseReason { get; set; }

    public int CurrentBlockIndex { get; set; }
    public int TotalBlocks { get; set; }
    public int CompletedBlocks { get; set; }
    public DateTime Date { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public string? ErrorText { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public List<AutomaticQueueRunBlockDto> Blocks { get; set; } = [];
}
