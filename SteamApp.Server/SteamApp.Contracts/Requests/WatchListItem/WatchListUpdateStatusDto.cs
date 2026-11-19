namespace SteamApp.Contracts.WatchListItem
{
    public sealed class WatchListUpdateStatusDto
    {
        public long Id { get; set; }
        public bool IsActive { get; set; }
    }
}
