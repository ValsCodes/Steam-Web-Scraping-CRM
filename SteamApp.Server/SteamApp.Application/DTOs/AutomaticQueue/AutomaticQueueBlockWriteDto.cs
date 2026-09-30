using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SteamApp.Application.DTOs.ManualCheck;
using SteamApp.Domain.Enums;

namespace SteamApp.Application.DTOs.AutomaticQueue;

public class AutomaticQueueBlockWriteDto
{
    public Guid Key { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public AutomaticQueueBlockTypeEnum Type { get; set; }

    public int? DelaySeconds { get; set; }
    public long? GameUrlId { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public AutomaticQueueTemplateModeEnum? TemplateMode { get; set; }

    public long? PresetId { get; set; }
    public ManualCheckPresetCombinationWriteDto? PresetCombination { get; set; }
    public AutomaticQueuePrivateTemplateDto? PrivateTemplate { get; set; }
    public bool BypassCache { get; set; }
    public List<long>? ProductIds { get; set; }
}
