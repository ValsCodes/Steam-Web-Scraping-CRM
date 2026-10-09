namespace SteamApp.WebAPI.Exceptions;

public sealed class AutomationAccessException(
    int statusCode,
    string message,
    DateTime? retryAtUtc = null) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public DateTime? RetryAtUtc { get; } = retryAtUtc;
}
