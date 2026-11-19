using SteamApp.Contracts.Common;
using SteamApp.Contracts.Enums;

namespace SteamApp.Contracts.ManualCheck;

public class ManualCheckPresetCombinationTermWriteDto
{
    public long PresetId { get; set; }

    [StringEncodedEnum]
    public ManualCheckPresetCombinationOperatorEnum? Operator { get; set; }
}
