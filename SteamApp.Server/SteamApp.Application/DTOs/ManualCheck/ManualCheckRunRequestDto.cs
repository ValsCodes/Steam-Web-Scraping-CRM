using SteamApp.Application.DTOs.AutomaticQueue;

namespace SteamApp.Application.DTOs.ManualCheck;

public sealed class ManualCheckRunRequestDto
{
    public long GameUrlId { get; set; }
    public long? PresetId { get; set; }
    public ManualCheckPresetCombinationWriteDto? PresetCombination { get; set; }
    public AutomaticQueuePrivateTemplateDto? PrivateTemplate { get; set; }
    public bool BypassCache { get; set; }
    public List<long>? ProductIds { get; set; }
}
