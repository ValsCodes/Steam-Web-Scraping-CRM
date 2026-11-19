using System.ComponentModel.DataAnnotations;

namespace SteamApp.Domain.Entities;

public sealed class AutomaticQueueDefinition
{
    public const int NameMaxLength = 100;

    public long Id { get; set; }

    [MaxLength(450)]
    public string? UserId { get; set; }

    [Required]
    [MaxLength(NameMaxLength)]
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public ICollection<AutomaticQueueBlock> Blocks { get; set; } = [];
    public ICollection<AutomaticQueueRun> Runs { get; set; } = [];
}
