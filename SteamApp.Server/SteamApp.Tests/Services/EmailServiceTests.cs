using System.Net;
using Microsoft.Extensions.Options;
using SteamApp.Infrastructure.Services;
using SteamApp.Interfaces.Services;
using SteamApp.Tests.TestSupport;

namespace SteamApp.Tests.Services;

[TestFixture]
public sealed class EmailServiceTests
{
    [Test]
    public async Task SendAsync_BuildsMailtrapRequestWithBearerTokenAndMessageBody()
    {
        string? body = null;
        Uri? requestUri = null;
        string? authScheme = null;
        string? authParameter = null;
        HttpMethod? method = null;

        var handler = new HttpMessageHandlerStub((request, _) =>
        {
            requestUri = request.RequestUri;
            method = request.Method;
            authScheme = request.Headers.Authorization?.Scheme;
            authParameter = request.Headers.Authorization?.Parameter;
            body = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://sandbox.api.mailtrap.io/")
        };
        var service = new EmailService(
            Options.Create(new EmailOptions
            {
                ApiToken = "secret-token",
                SandboxID = "123"
            }),
            client);

        await service.SendAsync(new EmailMessage(
            "user@example.com",
            "Price reached",
            "The watched price dropped."));

        Assert.Multiple(() =>
        {
            Assert.That(method, Is.EqualTo(HttpMethod.Post));
            Assert.That(requestUri?.ToString(), Is.EqualTo("https://sandbox.api.mailtrap.io/api/send/123"));
            Assert.That(authScheme, Is.EqualTo("Bearer"));
            Assert.That(authParameter, Is.EqualTo("secret-token"));
            Assert.That(body, Does.Contain("user@example.com"));
            Assert.That(body, Does.Contain("Price reached"));
            Assert.That(body, Does.Contain("The watched price dropped."));
        });
    }

    [Test]
    public void SendAsync_RethrowsCallerCancellation()
    {
        var service = new EmailService(
            Options.Create(new EmailOptions
            {
                ApiToken = "secret-token",
                SandboxID = "123"
            }),
            new HttpClient(HttpMessageHandlerStub.Ok())
            {
                BaseAddress = new Uri("https://sandbox.api.mailtrap.io/")
            });
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.That(
            async () => await service.SendAsync(
                new EmailMessage("user@example.com", "Subject", "Body"),
                cts.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }
}
