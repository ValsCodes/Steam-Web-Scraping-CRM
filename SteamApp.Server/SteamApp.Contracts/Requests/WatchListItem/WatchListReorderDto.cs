namespace SteamApp.Contracts.WatchListItem;

public sealed class WatchListReorderDto
{
    public IReadOnlyList<long>? OrderedIds { get; set; }
}
