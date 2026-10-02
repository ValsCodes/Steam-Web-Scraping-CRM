using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using SteamApp.WebAPI;

namespace SteamApp.Tests.ErrorHandling;

[TestFixture]
public sealed class GlobalExceptionHandlerTests
{
    [Test]
    public async Task ExceptionHandler_UnhandledException_ReturnsRedactedProblemDetails()
    {
        const string sensitiveMessage = "sensitive exception details";
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        await using var app = builder.Build();
        app.UseExceptionHandler();
        app.MapGet(
            "/throws",
            (HttpContext _) => Task.FromException(new InvalidOperationException(sensitiveMessage)));
        await app.StartAsync();
        using var client = app.GetTestClient();

        var response = await client.GetAsync("/throws");
        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
            Assert.That(json.RootElement.GetProperty("status").GetInt32(), Is.EqualTo(500));
            Assert.That(json.RootElement.GetProperty("instance").GetString(), Is.EqualTo("/throws"));
            Assert.That(json.RootElement.GetProperty("traceId").GetString(), Is.Not.Empty);
            Assert.That(body, Does.Not.Contain(sensitiveMessage));
            Assert.That(body, Does.Not.Contain(nameof(InvalidOperationException)));
        });
    }

    [Test]
    public async Task TryHandleAsync_RequestAbortedCancellation_DoesNotHandleException()
    {
        using var requestAborted = new CancellationTokenSource();
        requestAborted.Cancel();
        var context = new DefaultHttpContext
        {
            RequestAborted = requestAborted.Token
        };
        var handler = new GlobalExceptionHandler();

        var handled = await handler.TryHandleAsync(
            context,
            new OperationCanceledException(requestAborted.Token),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(handled, Is.False);
            Assert.That(context.Response.HasStarted, Is.False);
            Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        });
    }
}
