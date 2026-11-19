using System.ComponentModel.DataAnnotations;

namespace SteamApp.Contracts.ItemGroup;

public sealed class ItemGroupCreateDto
{
    public long GameId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
}
