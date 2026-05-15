using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SteamApp.Infrastructure.Services;
using SteamApp.Interfaces.Services;

namespace SteamApp.Tests.Services;

[TestFixture]
public sealed class TransientRetryPolicyServiceTests
{
    [Test]
    public async Task ExecuteAsync_RetriesTransientExceptionAndReturnsResult()
    {
        var attempts = 0;
        var service = CreateService(maxAttempts: 3);

        var result = await service.ExecuteAsync(
            "test operation",
            _ =>
            {
                attempts++;

                if (attempts < 3)
                {
                    throw new HttpRequestException(
                        "Service unavailable.",
                        null,
                        HttpStatusCode.ServiceUnavailable);
                }

                return Task.FromResult("ok");
            });

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("ok"));
            Assert.That(attempts, Is.EqualTo(3));
        });
    }

    [Test]
    public void ExecuteAsync_DoesNotRetryNonTransientException()
    {
        var attempts = 0;
        var service = CreateService(maxAttempts: 3);

        Assert.That(
            async () => await service.ExecuteAsync(
                "validation operation",
                _ =>
                {
                    attempts++;
                    throw new InvalidOperationException("Permanent failure.");
                }),
            Throws.TypeOf<InvalidOperationException>());

        Assert.That(attempts, Is.EqualTo(1));
    }

    [Test]
    public void ExecuteAsync_DoesNotRetryCallerCancellation()
    {
        var attempts = 0;
        var service = CreateService(maxAttempts: 3);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.That(
            async () => await service.ExecuteAsync(
                "cancelled operation",
                _ =>
                {
                    attempts++;
                    return Task.CompletedTask;
                },
                cts.Token),
            Throws.InstanceOf<OperationCanceledException>());

        Assert.That(attempts, Is.EqualTo(0));
    }

    [Test]
    public void IsTransient_ClassifiesCommonHttpAndNetworkFailures()
    {
        var service = CreateService();

        Assert.Multiple(() =>
        {
            Assert.That(
                service.IsTransient(new HttpRequestException("Network failure.")),
                Is.True);
            Assert.That(
                service.IsTransient(new HttpRequestException("Too many requests.", null, HttpStatusCode.TooManyRequests)),
                Is.True);
            Assert.That(
                service.IsTransient(new HttpRequestException("Bad request.", null, HttpStatusCode.BadRequest)),
                Is.False);
            Assert.That(
                service.IsTransient(new TimeoutException()),
                Is.True);
        });
    }

    private static TransientRetryPolicyService CreateService(int maxAttempts = 3)
    {
        return new TransientRetryPolicyService(
            Options.Create(new TransientRetryPolicyOptions
            {
                MaxAttempts = maxAttempts,
                BaseDelayMilliseconds = 1,
                MaxDelayMilliseconds = 1,
                UseJitter = false
            }),
            NullLogger<TransientRetryPolicyService>.Instance);
    }
}
