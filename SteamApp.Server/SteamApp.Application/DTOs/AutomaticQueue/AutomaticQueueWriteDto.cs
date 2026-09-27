namespace SteamApp.Application.DTOs.AutomaticQueue;

public sealed class AutomaticQueueWriteDto
{
    public string Name { get; set; } = string.Empty;
    public List<AutomaticQueueBlockWriteDto> Blocks { get; set; } = [];
}
