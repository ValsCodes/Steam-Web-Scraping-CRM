namespace SteamApp.Application.DTOs.WatchListItem
{
    public sealed class WatchListUpdateDto
    {
        public long Id { get; set; }
        public int? Rating { get; set; }
        public string? BatchUrl { get; set; }
        public string? Name { get; set; }
        public DateOnly? ReleaseDate { get; set; }
        public string? Description { get; set; }
    }
}
