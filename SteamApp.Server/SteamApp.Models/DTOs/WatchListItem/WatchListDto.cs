namespace SteamApp.Models.DTOs.WatchList
{
    public sealed class WatchListDto
    {
        public long Id { get; set; }
        public long? GameId { get; set; }
        public long? GameUrlId { get; set; }
        public int? Rating { get; set; }
        public string BatchUrl { get; set; }
        public string Name { get; set; }
        public DateOnly ReleaseDate { get; set; }
        public string Description { get; set; }
    }
}
