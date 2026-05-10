namespace SteamApp.Application.DTOs.Game
{
    public class GameCreateDto
    {
        public string Name { get; set; } = string.Empty;

        public string BaseUrl { get; set; } = string.Empty;

        public string? PageUrl { get; set; }

        public long? InternalId { get; set; }

        public bool IsActive { get; set; }
    }
}
