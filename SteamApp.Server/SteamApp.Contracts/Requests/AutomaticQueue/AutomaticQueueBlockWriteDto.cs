using SteamApp.Contracts.Common;
using SteamApp.Contracts.ManualCheck;
using SteamApp.Contracts.Enums;

namespace SteamApp.Contracts.AutomaticQueue;

public class AutomaticQueueBlockWriteDto
{
    public Guid Key { get; set; }

    [StringEncodedEnum]
    public AutomaticQueueBlockTypeEnum Type { get; set; }

    public int? DelaySeconds { get; set; }
    public long? GameUrlId { get; set; }

    [StringEncodedEnum]
    public AutomaticQueueTemplateModeEnum? TemplateMode { get; set; }

    public long? PresetId { get; set; }
    public ManualCheckPresetCombinationWriteDto? PresetCombination { get; set; }
    public AutomaticQueuePrivateTemplateDto? PrivateTemplate { get; set; }
    public bool BypassCache { get; set; }
    public List<long>? ProductIds { get; set; }
}
