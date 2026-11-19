
namespace SteamApp.Contracts.ManualCheck;

public class ManualCheckPresetWriteDto
{
    public long GameId { get; set; }
    public long? ItemGroupId { get; set; }
    public string Name { get; set; } = string.Empty;

    public int ListingLimit { get; set; } = ManualCheckDefaults.ListingLimit;
    public ManualCheckPriceRangeDto? PriceRange { get; set; }
    public int? CooldownMinutes { get; set; }
    public int? CooldownSeconds { get; set; }
    public List<long> GameUrlIds { get; set; } = [];
    public List<ManualCheckCriterionDto> Criteria { get; set; } = [];
}
