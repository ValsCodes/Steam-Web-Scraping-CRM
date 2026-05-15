using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SteamApp.Interfaces.Services;
using System.Net;
using System.Net.Sockets;

namespace SteamApp.Infrastructure.Services;

public sealed class TransientRetryPolicyService(
    IOptions<TransientRetryPolicyOptions> options,
    ILogger<TransientRetryPolicyService> logger) : ITransientRetryPolicyService
{
    public Task ExecuteAsync(
        string operationName,
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync<object?>(
            operationName,
            async ct =>
            {
                await operation(ct).ConfigureAwait(false);
                return null;
            },
            cancellationToken);
    }

    public async Task<T> ExecuteAsync<T>(
        string operationName,
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
        ArgumentNullException.ThrowIfNull(operation);

        var retryOptions = Normalize(options.Value);

        for (var attempt = 1; ; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return await operation(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ShouldRetry(ex, attempt, retryOptions, cancellationToken))
            {
                var delay = CalculateDelay(attempt, retryOptions);

                logger.LogWarning(
                    ex,
                    "Transient failure while running {OperationName}. Retrying attempt {NextAttempt}/{MaxAttempts} in {DelayMilliseconds} ms.",
                    operationName,
                    attempt + 1,
                    retryOptions.MaxAttempts,
                    delay.TotalMilliseconds);

                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    public bool IsTransient(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return IsTransientException(exception);
    }

    private static bool ShouldRetry(
        Exception exception,
        int attempt,
        TransientRetryPolicyOptions retryOptions,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && cancellationToken.IsCancellationRequested)
        {
            return false;
        }

        return attempt < retryOptions.MaxAttempts &&
               IsTransientException(exception);
    }

    private static bool IsTransientException(Exception exception)
    {
        return exception switch
        {
            HttpRequestException httpRequest => IsTransientHttpStatus(httpRequest.StatusCode),
            TimeoutException => true,
            TaskCanceledException => true,
            OperationCanceledException => true,
            IOException ioException when ioException.InnerException is SocketException => true,
            SocketException => true,
            _ when exception.InnerException != null => IsTransientException(exception.InnerException),
            _ => false
        };
    }

    private static bool IsTransientHttpStatus(HttpStatusCode? statusCode)
    {
        if (statusCode == null)
        {
            return true;
        }

        var numericStatusCode = (int)statusCode.Value;

        return statusCode is HttpStatusCode.RequestTimeout or
               HttpStatusCode.TooManyRequests or
               HttpStatusCode.BadGateway or
               HttpStatusCode.ServiceUnavailable or
               HttpStatusCode.GatewayTimeout ||
               numericStatusCode >= 500 && statusCode != HttpStatusCode.NotImplemented;
    }

    private static TimeSpan CalculateDelay(int failedAttempt, TransientRetryPolicyOptions retryOptions)
    {
        var multiplier = Math.Pow(2, failedAttempt - 1);
        var delayMilliseconds = retryOptions.BaseDelayMilliseconds * multiplier;
        var cappedDelayMilliseconds = Math.Min(delayMilliseconds, retryOptions.MaxDelayMilliseconds);

        if (retryOptions.UseJitter)
        {
            cappedDelayMilliseconds *= Random.Shared.NextDouble() * 0.4 + 0.8;
        }

        return TimeSpan.FromMilliseconds(cappedDelayMilliseconds);
    }

    private static TransientRetryPolicyOptions Normalize(TransientRetryPolicyOptions options)
    {
        var maxAttempts = Math.Clamp(options.MaxAttempts, 1, 10);
        var baseDelayMilliseconds = Math.Clamp(options.BaseDelayMilliseconds, 1, 60_000);
        var maxDelayMilliseconds = Math.Clamp(
            Math.Max(options.MaxDelayMilliseconds, baseDelayMilliseconds),
            baseDelayMilliseconds,
            300_000);

        return new TransientRetryPolicyOptions
        {
            MaxAttempts = maxAttempts,
            BaseDelayMilliseconds = baseDelayMilliseconds,
            MaxDelayMilliseconds = maxDelayMilliseconds,
            UseJitter = options.UseJitter
        };
    }

}
