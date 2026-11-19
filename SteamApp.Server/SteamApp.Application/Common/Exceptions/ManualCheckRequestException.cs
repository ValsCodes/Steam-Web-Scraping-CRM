namespace SteamApp.Application.Common.Exceptions;

public sealed class ManualCheckRequestException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
