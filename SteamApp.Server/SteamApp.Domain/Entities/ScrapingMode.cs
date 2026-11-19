namespace SteamApp.Domain.Entities;

public sealed class ScrapingMode
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<GameUrl> GameUrls { get; set; } = [];
}
