using SteamApp.Domain.Entities;

namespace SteamApp.Application.DTOs.ManualCheck;

public class ManualCheckPresetCombinationWriteDto
{
    public int ListingLimit { get; set; } = ManualCheckPreset.DefaultListingLimit;
    public ManualCheckPriceRangeDto? PriceRange { get; set; }
    public int? CooldownMinutes { get; set; }
    public int? CooldownSeconds { get; set; }
    public List<ManualCheckPresetCombinationTermWriteDto> Terms { get; set; } = [];
}
