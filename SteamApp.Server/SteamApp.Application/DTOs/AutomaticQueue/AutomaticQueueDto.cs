namespace SteamApp.Application.DTOs.AutomaticQueue;

public sealed class AutomaticQueueDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<AutomaticQueueBlockDto> Blocks { get; set; } = [];
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public long? ActiveRunId { get; set; }
    public string Scope { get; set; } = "personal";
    public bool CanEdit { get; set; }
    public bool CanClone { get; set; }
    public bool CanRun { get; set; } = true;
}
