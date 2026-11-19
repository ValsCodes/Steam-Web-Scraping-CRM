using SteamApp.Contracts.Common;
using SteamApp.Contracts.Enums;

namespace SteamApp.Contracts.ManualCheck;

public sealed class ManualCheckPriceRangeDto
{
    [StringEncodedEnum]
    public ManualCheckPriceRangeModeEnum Mode { get; set; }
    public long? MinimumPriceMinorUnits { get; set; }
    public long? MaximumPriceMinorUnits { get; set; }
}
