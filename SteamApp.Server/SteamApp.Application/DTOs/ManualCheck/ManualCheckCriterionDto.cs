namespace SteamApp.Application.DTOs.ManualCheck;

public sealed class ManualCheckCriterionDto
{
    public long? ConditionOperatorId { get; set; }
    public string? ConditionOperatorName { get; set; }
    public int OpenGroupCount { get; set; }
    public int CloseGroupCount { get; set; }
    public string? NameContains { get; set; }
    public string? ValueContains { get; set; }
}
