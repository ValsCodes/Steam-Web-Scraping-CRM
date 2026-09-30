using SteamApp.Domain.Entities;

namespace SteamApp.Application.DTOs.ManualCheck;

public sealed class ManualCheckPresetCombinationDto
{
    public int ListingLimit { get; set; } = ManualCheckPreset.DefaultListingLimit;
    public int? CooldownMinutes { get; set; }
    public int? CooldownSeconds { get; set; }
    public List<ManualCheckPresetCombinationTermDto> Terms { get; set; } = [];
}
