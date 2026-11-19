namespace SteamApp.Contracts.ItemGroup;

public sealed class ItemGroupDto
{
    public long Id { get; set; }
    public long GameId { get; set; }
    public string Name { get; set; } = string.Empty;
}
