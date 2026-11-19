using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SteamApp.Infrastructure.Messaging.RabbitMq.Options;

namespace SteamApp.Infrastructure.Messaging.RabbitMq.Connection;

public sealed class RabbitMqConnection(IOptions<RabbitMqOptions> options) : IAsyncDisposable
{
    private readonly RabbitMqOptions _options = options.Value;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private volatile IConnection? _connection;

    public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection is { } connection)
        {
            return connection;
        }

        await _connectionLock.WaitAsync(cancellationToken);

        try
        {
            if (_connection is { } existing)
            {
                return existing;
            }

            ConnectionFactory factory = new()
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                ClientProvidedName = "dotnet-9-webapi-test"
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);

            return _connection;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            
            await _connection.DisposeAsync();
        }

        _connectionLock.Dispose();
    }
}
