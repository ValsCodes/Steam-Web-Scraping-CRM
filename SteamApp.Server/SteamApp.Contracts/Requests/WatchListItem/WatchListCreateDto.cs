namespace SteamApp.Contracts.WatchListItem
{
    public sealed class WatchListCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Url { get; set; }
        public bool IsActive { get; set; }
    }
}
