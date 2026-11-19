using SteamApp.Contracts.Common;

namespace SteamApp.Contracts.Game
{
    public class GameDto : BaseDto
    {
        public string BaseUrl { get; set; } = string.Empty;

        public string? PageUrl { get; set; }

        public long? InternalId { get; set; }

        public bool IsActive { get; set; }
    }
}
