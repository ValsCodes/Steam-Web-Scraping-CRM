using SteamApp.Domain.Entities;

namespace SteamApp.Application.DTOs.ManualCheck;

public sealed class ManualCheckSetupDto
{
    public long? PresetId { get; set; }
    public string PresetName { get; set; } = string.Empty;
    public long GameId { get; set; }
    public string? GameName { get; set; }
    public long GameUrlId { get; set; }
    public string? GameUrlName { get; set; }

    public int ListingLimit { get; set; } = ManualCheckPreset.DefaultListingLimit;
    public int? CooldownMinutes { get; set; }
    public int? CooldownSeconds { get; set; }
    public bool BypassCache { get; set; }
    public List<ManualCheckCriterionDto> Criteria { get; set; } = [];
    public List<ManualCheckProductInputDto> Products { get; set; } = [];
    public DateTime RequestedAtUtc { get; set; }
}
