using SteamApp.Contracts.AutomaticQueue;

namespace SteamApp.Contracts.ManualCheck;

public sealed class ManualCheckRunRequestDto
{
    public long GameUrlId { get; set; }
    public long? PresetId { get; set; }
    public ManualCheckPresetCombinationWriteDto? PresetCombination { get; set; }
    public AutomaticQueuePrivateTemplateDto? PrivateTemplate { get; set; }
    public bool BypassCache { get; set; }
    public List<long>? ProductIds { get; set; }
}
