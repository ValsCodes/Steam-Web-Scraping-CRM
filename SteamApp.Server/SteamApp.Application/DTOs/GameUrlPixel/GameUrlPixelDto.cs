namespace SteamApp.Application.DTOs.GameUrlPixel
{
    public class GameUrlPixelDto
    {
        public long PixelId { get; set; }

        public string PixelName { get; set; } = null!;

        public long GameUrlId { get; set; }

        public string GameUrlName { get; set; } = null!;
    }
}
