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
    public Game Game { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    public ICollection<ProductTags> ProductTags { get; set; } = [];
}
