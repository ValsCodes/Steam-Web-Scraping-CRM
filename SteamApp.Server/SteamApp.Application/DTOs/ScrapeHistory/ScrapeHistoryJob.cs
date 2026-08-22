using SteamApp.Domain.Enums;

namespace SteamApp.Application.DTOs.ScrapeHistory;

public sealed record ScrapeHistoryJob(
    long Id,
    string? UserId,
    string Endpoint,
    string ScrapeType,
    long GameUrlId,
    short Page,
    ScrapeJobStatusEnum Status,
    string? CorrelationId);
