using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Domain.Entities;

[Table("automatic_queue_definition")]
public sealed class AutomaticQueueDefinition
{
    public const int NameMaxLength = 100;

    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [MaxLength(450)]
    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(NameMaxLength)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("created_at_utc")]
    public DateTime CreatedAtUtc { get; set; }

    [Column("updated_at_utc")]
    public DateTime UpdatedAtUtc { get; set; }

    public ICollection<AutomaticQueueBlock> Blocks { get; set; } = [];
    public ICollection<AutomaticQueueRun> Runs { get; set; } = [];
}
