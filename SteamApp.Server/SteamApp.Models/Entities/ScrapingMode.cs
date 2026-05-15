using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Domain.Entities;

[Table("scraping_mode")]
public sealed class ScrapingMode
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [InverseProperty(nameof(GameUrl.ScrapingMode))]
    public ICollection<GameUrl> GameUrls { get; set; } = [];
}
