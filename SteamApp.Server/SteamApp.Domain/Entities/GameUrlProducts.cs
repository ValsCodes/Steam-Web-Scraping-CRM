namespace SteamApp.Domain.Entities
{
    public class GameUrlProducts
    {
        public long GameUrlId { get; set; }
        public GameUrl GameUrl { get; set; } = null!;

        public long ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int CurrentStock { get; set; }
    }
}
