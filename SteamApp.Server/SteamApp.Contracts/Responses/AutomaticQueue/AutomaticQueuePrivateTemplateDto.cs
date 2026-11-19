using SteamApp.Contracts.Common;
using SteamApp.Contracts.ManualCheck;

namespace SteamApp.Contracts.AutomaticQueue;

public sealed class AutomaticQueuePrivateTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public int ListingLimit { get; set; } = ManualCheckDefaults.ListingLimit;
    public ManualCheckPriceRangeDto? PriceRange { get; set; }
    public int? CooldownMinutes { get; set; }
    public int? CooldownSeconds { get; set; }
    public List<ManualCheckCriterionDto> Criteria { get; set; } = [];
}
