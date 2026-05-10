using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SteamApp.Tests.TestSupport;

public sealed class OneShotHttpServer : IAsyncDisposable
{
    private readonly TcpListener listener;
    private readonly Task serverTask;

    private OneShotHttpServer(TcpListener listener, Task serverTask, Uri uri)
    {
        this.listener = listener;
        this.serverTask = serverTask;
        Uri = uri;
    }

    public Uri Uri { get; }

    public static OneShotHttpServer StartJson(string json)
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var uri = new Uri($"http://127.0.0.1:{port}/listings?page={{0}}");

        var task = Task.Run(async () =>
        {
            using var client = await listener.AcceptTcpClientAsync();
            await using var stream = client.GetStream();
            using var reader = new StreamReader(
                stream,
                Encoding.ASCII,
                leaveOpen: true);

            while (!string.IsNullOrEmpty(await reader.ReadLineAsync()))
            {
            }

            var body = Encoding.UTF8.GetBytes(json);
            var header = Encoding.ASCII.GetBytes(
                "HTTP/1.1 200 OK\r\n" +
                "Content-Type: application/json\r\n" +
                $"Content-Length: {body.Length}\r\n" +
                "Connection: close\r\n\r\n");

            await stream.WriteAsync(header);
            await stream.WriteAsync(body);
        });

        return new OneShotHttpServer(listener, task, uri);
    }

    public async ValueTask DisposeAsync()
    {
        listener.Stop();
        await serverTask.WaitAsync(TimeSpan.FromSeconds(5));
    }
}
