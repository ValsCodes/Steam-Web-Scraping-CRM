
namespace SteamApp.Contracts.ManualCheck;

public sealed class ManualCheckPresetCombinationDto
{
    public int ListingLimit { get; set; } = ManualCheckDefaults.ListingLimit;
    public ManualCheckPriceRangeDto? PriceRange { get; set; }
    public int? CooldownMinutes { get; set; }
    public int? CooldownSeconds { get; set; }
    public List<ManualCheckPresetCombinationTermDto> Terms { get; set; } = [];
}
