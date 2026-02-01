namespace SteamApp.Application.DTOs.Tag
{
    public class TagDto
    {
        public long Id { get; set; }
        public long GameId { get; set; }
        public string GameName { get; set; } = null!;
        public string? Name { get; set; }
    }
}
