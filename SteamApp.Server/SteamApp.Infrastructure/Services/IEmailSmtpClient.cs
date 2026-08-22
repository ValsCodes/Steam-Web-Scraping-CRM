using MailKit.Security;
using MimeKit;

namespace SteamApp.Infrastructure.Services;

internal interface IEmailSmtpClient : IAsyncDisposable
{
    void AllowInvalidServerCertificate();

    Task ConnectAsync(
        string host,
        int port,
        SecureSocketOptions options,
        CancellationToken cancellationToken);

    Task AuthenticateAsync(
        string userName,
        string password,
        CancellationToken cancellationToken);

    Task SendAsync(
        MimeMessage message,
        CancellationToken cancellationToken);

    Task DisconnectAsync(
        bool quit,
        CancellationToken cancellationToken);
}
