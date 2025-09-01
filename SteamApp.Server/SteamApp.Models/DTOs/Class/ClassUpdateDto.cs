using System.ComponentModel.DataAnnotations;

namespace SteamApp.Models.DTOs.Class
{
    public class ClassUpdateDto
    {
        [Required]
        public long Id { get; set; }

        public string? Name { get; set; }

        public long? GameId { get; set; }
    }
}
