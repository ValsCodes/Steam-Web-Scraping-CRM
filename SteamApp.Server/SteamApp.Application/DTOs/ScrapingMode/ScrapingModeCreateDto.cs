using System.ComponentModel.DataAnnotations;

namespace SteamApp.Application.DTOs.ScrapingMode
{
    public sealed class ScrapingModeCreateDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
    }
}
