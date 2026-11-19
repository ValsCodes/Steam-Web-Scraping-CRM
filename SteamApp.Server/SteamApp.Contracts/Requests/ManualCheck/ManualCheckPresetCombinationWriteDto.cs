
namespace SteamApp.Contracts.ManualCheck;

public class ManualCheckPresetCombinationWriteDto
{
    public int ListingLimit { get; set; } = ManualCheckDefaults.ListingLimit;
    public ManualCheckPriceRangeDto? PriceRange { get; set; }
    public int? CooldownMinutes { get; set; }
    public int? CooldownSeconds { get; set; }
    public List<ManualCheckPresetCombinationTermWriteDto> Terms { get; set; } = [];
}
