namespace SteamApp.Application.DTOs.ManualCheck;

public sealed class ManualCheckRunResultsDto
{
    public List<ManualCheckProductResultDto> Matches { get; set; } = [];
    public List<ManualCheckProductTraceDto> ProductTraces { get; set; } = [];
    public List<ManualCheckProductErrorDto> Errors { get; set; } = [];
}
