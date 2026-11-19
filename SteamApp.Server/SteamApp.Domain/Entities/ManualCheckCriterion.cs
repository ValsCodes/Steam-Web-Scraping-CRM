using System.ComponentModel.DataAnnotations;

namespace SteamApp.Domain.Entities;

public sealed class ManualCheckCriterion
{
    public const int TermMaxLength = 200;

    public long Id { get; set; }

    public long ManualCheckPresetId { get; set; }

    public ManualCheckPreset ManualCheckPreset { get; set; } = null!;

    public long? ConditionOperatorId { get; set; }

    public ManualCheckConditionOperator? ConditionOperator { get; set; }

    public int SortOrder { get; set; }

    public int OpenGroupCount { get; set; }

    public int CloseGroupCount { get; set; }

    [MaxLength(TermMaxLength)]
    public string? NameContains { get; set; }

    [MaxLength(TermMaxLength)]
    public string? ValueContains { get; set; }
}
