namespace SteamApp.Application.DTOs.AutomaticQueue;

public sealed class AutomaticQueueBlockDto : AutomaticQueueBlockWriteDto
{
    public long Id { get; set; }
    public int SortOrder { get; set; }
    public long? GameId { get; set; }
    public string? GameName { get; set; }
    public string? GameUrlName { get; set; }
    public string? PresetName { get; set; }
}
