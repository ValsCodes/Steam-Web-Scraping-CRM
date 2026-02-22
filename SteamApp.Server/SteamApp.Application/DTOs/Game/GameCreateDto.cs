namespace SteamApp.Application.DTOs.Game
{
    public class GameCreateDto
    {
        public string Name { get; set; }
        public string BaseUrl { get; set; }

        public string? PageUrl { get; set; }
    }
}
