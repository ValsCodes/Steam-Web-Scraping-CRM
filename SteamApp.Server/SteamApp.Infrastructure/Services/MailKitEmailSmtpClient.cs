using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace SteamApp.Infrastructure.Services;

internal sealed class MailKitEmailSmtpClient : IEmailSmtpClient
{
    private readonly SmtpClient client = new();

    public void AllowInvalidServerCertificate()
    {
        client.ServerCertificateValidationCallback = (_, _, _, _) => true;
    }

    public Task ConnectAsync(
        string host,
        int port,
        SecureSocketOptions options,
        CancellationToken cancellationToken)
    {
        return client.ConnectAsync(host, port, options, cancellationToken);
    }

    public Task AuthenticateAsync(
        string userName,
        string password,
        CancellationToken cancellationToken)
    {
        return client.AuthenticateAsync(userName, password, cancellationToken);
    }

    public Task SendAsync(
        MimeMessage message,
        CancellationToken cancellationToken)
    {
        return client.SendAsync(message, cancellationToken);
    }

    public Task DisconnectAsync(
        bool quit,
        CancellationToken cancellationToken)
    {
        return client.DisconnectAsync(quit, cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        client.Dispose();
        return ValueTask.CompletedTask;
    }
}
