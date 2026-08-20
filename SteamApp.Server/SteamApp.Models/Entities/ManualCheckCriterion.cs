using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Domain.Entities;

[Table("manual_check_criterion")]
public sealed class ManualCheckCriterion
{
    public const int TermMaxLength = 200;

    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("manual_check_preset_id")]
    public long ManualCheckPresetId { get; set; }

    public ManualCheckPreset ManualCheckPreset { get; set; } = null!;

    [Column("condition_operator_id")]
    public long? ConditionOperatorId { get; set; }

    public ManualCheckConditionOperator? ConditionOperator { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [MaxLength(TermMaxLength)]
    [Column("name_contains")]
    public string? NameContains { get; set; }

    [MaxLength(TermMaxLength)]
    [Column("value_contains")]
    public string? ValueContains { get; set; }
}
