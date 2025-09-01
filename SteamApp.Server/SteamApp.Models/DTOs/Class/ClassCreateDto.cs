using System.ComponentModel.DataAnnotations;

namespace SteamApp.Models.DTOs.Class
{
    public class ClassCreateDto
    {
        [Required]
        public string Name { get; set; } = default!;

        [Required]
        public long GameId { get; set; }
    }
}
