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
