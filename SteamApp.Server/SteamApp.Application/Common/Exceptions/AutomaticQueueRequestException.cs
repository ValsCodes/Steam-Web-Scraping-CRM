namespace SteamApp.Application.Common.Exceptions;

public sealed class AutomaticQueueRequestException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
