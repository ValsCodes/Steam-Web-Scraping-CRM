using SteamApp.Application.DTOs.ManualCheck;
using SteamApp.Domain.Entities;

namespace SteamApp.Application.DTOs.AutomaticQueue;

public sealed class AutomaticQueuePrivateTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public int ListingLimit { get; set; } = ManualCheckPreset.DefaultListingLimit;
    public ManualCheckPriceRangeDto? PriceRange { get; set; }
    public int? CooldownMinutes { get; set; }
    public int? CooldownSeconds { get; set; }
    public List<ManualCheckCriterionDto> Criteria { get; set; } = [];
}
