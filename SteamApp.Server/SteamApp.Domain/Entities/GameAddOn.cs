namespace SteamApp.Domain.Entities
{
    public class GameAddOn
    {
        public long Id { get; set; }

        public string? Name { get; set; }

        public long GameId { get; set; }
        public Game Game { get; set; } = null!;

        public double? Price { get; set; }
    }
}
