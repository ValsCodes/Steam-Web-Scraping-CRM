using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Domain.Entities;

[Table("manual_check_condition_operator")]
public sealed class ManualCheckConditionOperator
{
    public const int NameMaxLength = 20;

    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [MaxLength(NameMaxLength)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    public ICollection<ManualCheckCriterion> Criteria { get; set; } = [];
}
