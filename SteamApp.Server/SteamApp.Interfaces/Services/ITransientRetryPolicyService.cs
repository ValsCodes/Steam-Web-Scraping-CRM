namespace SteamApp.Interfaces.Services;

public interface ITransientRetryPolicyService
{
    Task ExecuteAsync(
        string operationName,
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default);

    Task<T> ExecuteAsync<T>(
        string operationName,
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default);

    bool IsTransient(Exception exception);
}

public sealed class TransientRetryPolicyOptions
{
    public const string SectionName = "TransientRetryPolicy";

    public int MaxAttempts { get; set; } = 3;
    public int BaseDelayMilliseconds { get; set; } = 200;
    public int MaxDelayMilliseconds { get; set; } = 2_000;
    public bool UseJitter { get; set; } = true;
}
