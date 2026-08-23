using SteamApp.Domain.Entities;

namespace SteamApp.Application.DTOs.ManualCheck;

public class ManualCheckPresetWriteDto
{
    public long GameId { get; set; }
    public long? ItemGroupId { get; set; }
    public string Name { get; set; } = string.Empty;

    public int ListingLimit { get; set; } = ManualCheckPreset.DefaultListingLimit;
    public int? CooldownMinutes { get; set; }
    public int? CooldownSeconds { get; set; }
    public List<ManualCheckCriterionDto> Criteria { get; set; } = [];
}
