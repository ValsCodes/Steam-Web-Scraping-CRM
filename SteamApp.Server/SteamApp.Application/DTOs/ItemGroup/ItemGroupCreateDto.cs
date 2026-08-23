using System.ComponentModel.DataAnnotations;

namespace SteamApp.Application.DTOs.ItemGroup;

public sealed class ItemGroupCreateDto
{
    public long GameId { get; set; }

    [Required]
    [MaxLength(SteamApp.Domain.Entities.ItemGroup.NameMaxLength)]
    public string Name { get; set; } = string.Empty;
}
