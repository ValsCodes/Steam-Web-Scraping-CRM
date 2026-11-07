namespace SteamApp.Application.DTOs.WatchListItem;

public sealed class WatchListReorderDto
{
    public IReadOnlyList<long>? OrderedIds { get; set; }
}
