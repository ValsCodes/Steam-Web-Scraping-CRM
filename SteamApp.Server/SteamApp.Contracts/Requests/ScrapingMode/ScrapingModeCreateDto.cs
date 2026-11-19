using System.ComponentModel.DataAnnotations;

namespace SteamApp.Contracts.ScrapingMode
{
    public sealed class ScrapingModeCreateDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
    }
}
