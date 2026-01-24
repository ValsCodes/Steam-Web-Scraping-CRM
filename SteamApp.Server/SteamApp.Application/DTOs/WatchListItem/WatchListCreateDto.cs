namespace SteamApp.Application.DTOs.WatchListItem
{
    public sealed class WatchListCreateDto
    {
        public long? ProductId { get; set; }
        public long? GameUrlId { get; set; }
        public int? Rating { get; set; }
        public long? BatchNumber { get; set; }
        public string Name { get; set; }
        public DateOnly ReleaseDate { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}
