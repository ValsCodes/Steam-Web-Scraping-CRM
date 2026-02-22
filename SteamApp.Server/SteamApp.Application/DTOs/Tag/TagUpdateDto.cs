using System.ComponentModel.DataAnnotations;

namespace SteamApp.Application.DTOs.Tag
{
    public class TagUpdateDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = null!;
    }
}
