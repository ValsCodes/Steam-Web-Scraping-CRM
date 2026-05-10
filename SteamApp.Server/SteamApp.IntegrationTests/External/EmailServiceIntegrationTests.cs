using System.Net;
using Microsoft.Extensions.Options;
using SteamApp.Infrastructure.Services;
using SteamApp.Interfaces.Services;

namespace SteamApp.IntegrationTests.External;

[TestFixture]
public sealed class EmailServiceIntegrationTests
{
    [Test]
    public async Task EmailServiceSendsExpectedMailtrapRequestToFakeHttpServer()
    {
        HttpRequestMessage? capturedRequest = null;
        string? capturedBody = null;
        var handler = new RecordingHandler(async request =>
        {
            capturedRequest = request;
            capturedBody = request.Content == null
                ? null
                : await request.Content.ReadAsStringAsync();

            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://sandbox.api.mailtrap.io/")
        };
        var service = new EmailService(
            Options.Create(new EmailOptions
            {
                ApiToken = "mailtrap-token",
                SandboxID = "987"
            }),
            client);

        await service.SendAsync(new EmailMessage(
            "person@example.com",
            "Price reached",
            "The item dropped."));

        Assert.Multiple(() =>
        {
            Assert.That(capturedRequest?.Method, Is.EqualTo(HttpMethod.Post));
            Assert.That(capturedRequest?.RequestUri?.ToString(), Is.EqualTo("https://sandbox.api.mailtrap.io/api/send/987"));
            Assert.That(capturedRequest?.Headers.Authorization?.Scheme, Is.EqualTo("Bearer"));
            Assert.That(capturedRequest?.Headers.Authorization?.Parameter, Is.EqualTo("mailtrap-token"));
            Assert.That(capturedBody, Does.Contain("person@example.com"));
            Assert.That(capturedBody, Does.Contain("Price reached"));
            Assert.That(capturedBody, Does.Contain("The item dropped."));
        });
    }

    [Test]
    public void EmailServiceRethrowsCallerCancellation()
    {
        var service = new EmailService(
            Options.Create(new EmailOptions
            {
                ApiToken = "mailtrap-token",
                SandboxID = "987"
            }),
            new HttpClient(new RecordingHandler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK))))
            {
                BaseAddress = new Uri("https://sandbox.api.mailtrap.io/")
            });
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.That(
            async () => await service.SendAsync(
                new EmailMessage("person@example.com", "Subject", "Body"),
                cts.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }

    private sealed class RecordingHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return handler(request);
        }
    }
}
