using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Domain.Entities;

[Table("item_group")]
public sealed class ItemGroup
{
    public const int NameMaxLength = 255;

    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("game_id")]
    [ForeignKey(nameof(Game))]
    public long GameId { get; set; }

    [InverseProperty(nameof(Game.ItemGroups))]
    public Game Game { get; set; } = null!;

    [Required]
    [MaxLength(NameMaxLength)]
    [Column("name")]
    public string Name { get; set; } = null!;

    [MaxLength(450)]
    [Column("user_id")]
    public string? UserId { get; set; }

    public ICollection<Tag> Tags { get; set; } = [];
    public ICollection<ManualCheckPreset> ManualCheckPresets { get; set; } = [];
}
