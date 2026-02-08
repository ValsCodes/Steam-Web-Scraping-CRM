namespace SteamApp.Application.DTOs.WatchList
{
    public sealed class WatchListDto
    {
        public long Id { get; set; }
        public long? ProductId { get; set; }
        public long? GameId { get; set; }
        public long? GameUrlId { get; set; }
        public long? BatchNumber { get; set; }
        public string Name { get; set; }
        public DateOnly ReleaseDate { get; set; }
        public string CustomUrl { get; set; }
        public bool IsActive { get; set; }
    }
}
