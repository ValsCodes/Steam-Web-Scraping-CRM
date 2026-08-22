namespace SteamApp.Application.DTOs.ManualCheck;

public sealed class ManualCheckProductErrorDto
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string FullUrl { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public string? ErrorType { get; set; }
    public int? HttpStatusCode { get; set; }
    public DateTime? OccurredAtUtc { get; set; }
}
