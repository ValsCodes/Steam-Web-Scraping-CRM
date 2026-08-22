namespace SteamApp.Application.DTOs.ManualCheck;

public sealed class ManualCheckDescriptionMatchDto
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public List<int> MatchedCriterionIndexes { get; set; } = [];
}
