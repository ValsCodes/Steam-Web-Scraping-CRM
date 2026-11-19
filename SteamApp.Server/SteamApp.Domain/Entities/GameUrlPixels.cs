namespace SteamApp.Domain.Entities
{
    public class GameUrlPixels
    {
        public long GameUrlId { get; set; }
        public GameUrl GameUrl { get; set; } = null!;

        public long PixelId { get; set; }
        public Pixel Pixel { get; set; } = null!;
    }
}
