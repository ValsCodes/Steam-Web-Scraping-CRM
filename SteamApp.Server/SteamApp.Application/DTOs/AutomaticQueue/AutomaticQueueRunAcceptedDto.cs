namespace SteamApp.Application.DTOs.AutomaticQueue;

public sealed class AutomaticQueueRunAcceptedDto
{
    public long RunId { get; set; }
    public AutomaticQueueRunDto Run { get; set; } = new();
}
