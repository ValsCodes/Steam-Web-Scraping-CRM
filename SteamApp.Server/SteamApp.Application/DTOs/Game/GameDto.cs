using SteamApp.Application.DTOs;

namespace SteamApp.Application.DTOs.Game
{
    public class GameDto : BaseDto
    {
        public string BaseUrl { get; set; }

        public string? PageUrl { get; set; }
    }
}
