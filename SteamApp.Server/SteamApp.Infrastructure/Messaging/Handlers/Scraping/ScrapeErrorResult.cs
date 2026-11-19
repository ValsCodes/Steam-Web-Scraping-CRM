using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SteamApp.Application.Features.Scraping;

namespace SteamApp.Infrastructure.Messaging.Handlers.Scraping;

public sealed record ScrapeErrorResult(int StatusCode, string Message, LogLevel LogLevel)
{
    public static ScrapeErrorResult FromException(string endpoint, Exception exception)
    {
        return endpoint == ScrapeOperation.ScrapePixelsEndpoint && exception is JsonSerializationException
            ? new ScrapeErrorResult(StatusCodes.Status400BadRequest, "Error: Invalid Listing", LogLevel.Warning)
            : new ScrapeErrorResult(
                StatusCodes.Status500InternalServerError,
                exception.Message,
                LogLevel.Error);
    }
}
