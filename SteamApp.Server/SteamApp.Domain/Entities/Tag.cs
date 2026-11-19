using System.ComponentModel.DataAnnotations;

namespace SteamApp.Domain.Entities;

public sealed class Tag
{
    public long Id { get; set; }

    public long GameId { get; set; }
    public Game Game { get; set; } = null!;

    public long? ItemGroupId { get; set; }

    public ItemGroup? ItemGroup { get; set; }

    public string? Name { get; set; }

    public bool IsActive { get; set; }

    [MaxLength(450)]
    public string? UserId { get; set; }

    public ICollection<ProductTags> ProductTags { get; set; } = [];
}
