using SteamApp.Application.DTOs;

namespace SteamApp.Application.DTOs.Game
{
    public class GameDto : BaseDto
    {
        public string BaseUrl { get; set; } = string.Empty;

        public string? PageUrl { get; set; }

        public long? InternalId { get; set; }

        public bool IsActive { get; set; }
    }
}
