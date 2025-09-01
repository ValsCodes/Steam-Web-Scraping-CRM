using System.ComponentModel.DataAnnotations;

namespace SteamApp.Models.DTOs.Game
{
    public class GameUpdateDto : BaseUpdateDto
    {
        [Required]
        public long Id { get; set; }
    }
}
