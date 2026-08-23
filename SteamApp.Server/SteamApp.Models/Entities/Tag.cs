using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Domain.Entities;

[Table("tag")]
public sealed class Tag
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("game_id")]
    [ForeignKey(nameof(Game))]
    public long GameId { get; set; }
    [InverseProperty(nameof(Game.Tags))]
    public Game Game { get; set; } = null!;

    [Column("item_group_id")]
    [ForeignKey(nameof(ItemGroup))]
    public long? ItemGroupId { get; set; }

    [InverseProperty(nameof(ItemGroup.Tags))]
    public ItemGroup? ItemGroup { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    [MaxLength(450)]
    [Column("user_id")]
    public string? UserId { get; set; }

    public ICollection<ProductTags> ProductTags { get; set; } = [];
}
