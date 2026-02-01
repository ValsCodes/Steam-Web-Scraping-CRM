using System.ComponentModel.DataAnnotations;

namespace SteamApp.Application.DTOs.Tag
{
    public class TagCreateDto
    {
        [Required]
        public long GameId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = null!;
    }
}
