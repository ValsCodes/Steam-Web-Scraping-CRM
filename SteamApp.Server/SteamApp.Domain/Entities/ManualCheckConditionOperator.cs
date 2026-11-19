using System.ComponentModel.DataAnnotations;

namespace SteamApp.Domain.Entities;

public sealed class ManualCheckConditionOperator
{
    public const int NameMaxLength = 20;

    public long Id { get; set; }

    [Required]
    [MaxLength(NameMaxLength)]
    public string Name { get; set; } = string.Empty;

    public ICollection<ManualCheckCriterion> Criteria { get; set; } = [];
}
