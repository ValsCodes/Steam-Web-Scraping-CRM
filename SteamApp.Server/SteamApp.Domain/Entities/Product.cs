using System.ComponentModel.DataAnnotations;

namespace SteamApp.Domain.Entities;

public sealed class Product
{
    public long Id { get; set; }

    public long GameId { get; set; }
    public Game Game { get; set; } = null!;

    public string? Name { get; set; }

    public int? Rating { get; set; }

    public bool IsActive { get; set; }

    [MaxLength(450)]
    public string? UserId { get; set; }

    public ICollection<GameUrlProducts> GameUrlsProducts { get; set; } = [];

    public ICollection<ProductTags> ProductTags { get; set; } = [];

    public ICollection<WatchList> WatchLists { get; set; } = [];
}
