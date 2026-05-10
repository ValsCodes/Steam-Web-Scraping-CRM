using System.Net;

namespace SteamApp.Tests.TestSupport;

public sealed class HttpMessageHandlerStub : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> handler;

    public HttpMessageHandlerStub(
        Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> handler)
    {
        this.handler = handler;
    }

    public HttpRequestMessage? LastRequest { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LastRequest = request;
        return Task.FromResult(handler(request, cancellationToken));
    }

    public static HttpMessageHandlerStub Ok()
    {
        return new HttpMessageHandlerStub((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK));
    }
}
