using System.Net.Sockets;
using MailKit.Security;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MimeKit;
using SteamApp.Infrastructure.Services;
using SteamApp.Interfaces.Services;

namespace SteamApp.Tests.Services;

[TestFixture]
public sealed class EmailServiceTests
{
    [Test]
    public async Task SendAsync_ConnectsWithStartTlsAuthenticatesAndBuildsMessage()
    {
        var client = new FakeEmailSmtpClient();
        var service = CreateService(new FakeEmailSmtpClientFactory(client));

        await service.SendAsync(new EmailMessage(
            "user@example.com",
            "Price reached",
            "The watched price dropped."));

        var sentMessage = client.SentMessage!;
        Assert.That(sentMessage.Body, Is.TypeOf<TextPart>());
        var sentBody = (TextPart)sentMessage.Body!;

        Assert.Multiple(() =>
        {
            Assert.That(client.Host, Is.EqualTo("mail.example.com"));
            Assert.That(client.Port, Is.EqualTo(587));
            Assert.That(client.SecureSocketOptions, Is.EqualTo(SecureSocketOptions.StartTls));
            Assert.That(client.UserName, Is.EqualTo("notifications@example.com"));
            Assert.That(client.Password, Is.EqualTo("secret-password"));
            Assert.That(client.Disconnected, Is.True);
            Assert.That(client.Disposed, Is.True);
            Assert.That(sentMessage.From.Mailboxes.Single().Address, Is.EqualTo("notifications@example.com"));
            Assert.That(sentMessage.From.Mailboxes.Single().Name, Is.EqualTo("SteamApp"));
            Assert.That(sentMessage.To.Mailboxes.Single().Address, Is.EqualTo("user@example.com"));
            Assert.That(sentMessage.Subject, Is.EqualTo("Price reached"));
            Assert.That(sentBody.Text, Is.EqualTo("The watched price dropped."));
        });
    }

    [Test]
    public void SendAsync_RethrowsCallerCancellation()
    {
        var service = CreateService(new FakeEmailSmtpClientFactory(new FakeEmailSmtpClient()));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.That(
            async () => await service.SendAsync(
                new EmailMessage("user@example.com", "Subject", "Body"),
                cts.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public async Task SendAsync_RetriesTransientSmtpFailures()
    {
        var failingClient = new FakeEmailSmtpClient
        {
            SendException = new SocketException((int)SocketError.TimedOut)
        };
        var succeedingClient = new FakeEmailSmtpClient();
        var factory = new FakeEmailSmtpClientFactory(failingClient, succeedingClient);
        var retryPolicy = new TransientRetryPolicyService(
            Options.Create(new TransientRetryPolicyOptions
            {
                MaxAttempts = 2,
                BaseDelayMilliseconds = 1,
                MaxDelayMilliseconds = 1,
                UseJitter = false
            }),
            NullLogger<TransientRetryPolicyService>.Instance);
        var service = CreateService(factory, retryPolicy);

        await service.SendAsync(new EmailMessage(
            "user@example.com",
            "Price reached",
            "The watched price dropped."));

        Assert.Multiple(() =>
        {
            Assert.That(factory.CreateCalls, Is.EqualTo(2));
            Assert.That(failingClient.SendAttempts, Is.EqualTo(1));
            Assert.That(succeedingClient.SendAttempts, Is.EqualTo(1));
            Assert.That(succeedingClient.SentMessage, Is.Not.Null);
        });
    }

    [Test]
    public async Task SendAsync_AllowsInvalidServerCertificateWhenConfigured()
    {
        var client = new FakeEmailSmtpClient();
        var service = CreateService(
            new FakeEmailSmtpClientFactory(client),
            options: new EmailOptions
            {
                Host = "localhost",
                Port = 587,
                UserName = "notifications@steamapp.test",
                Password = "secret-password",
                FromAddress = "notifications@steamapp.test",
                FromName = "SteamApp",
                UseStartTls = true,
                AllowInvalidCertificate = true
            });

        await service.SendAsync(new EmailMessage(
            "user@example.com",
            "Price reached",
            "The watched price dropped."));

        Assert.That(client.InvalidServerCertificateAllowed, Is.True);
    }

    private static EmailService CreateService(
        IEmailSmtpClientFactory factory,
        ITransientRetryPolicyService? retryPolicy = null,
        EmailOptions? options = null)
    {
        return new EmailService(
            Options.Create(options ?? new EmailOptions
            {
                Host = "mail.example.com",
                Port = 587,
                UserName = "notifications@example.com",
                Password = "secret-password",
                FromAddress = "notifications@example.com",
                FromName = "SteamApp",
                UseStartTls = true
            }),
            factory,
            retryPolicy ?? new TransientRetryPolicyService(
                Options.Create(new TransientRetryPolicyOptions()),
                NullLogger<TransientRetryPolicyService>.Instance),
            NullLogger<EmailService>.Instance);
    }

    private sealed class FakeEmailSmtpClientFactory(
        params FakeEmailSmtpClient[] clients) : IEmailSmtpClientFactory
    {
        private readonly Queue<FakeEmailSmtpClient> clients = new(clients);

        public int CreateCalls { get; private set; }

        public IEmailSmtpClient Create()
        {
            CreateCalls++;
            return clients.Dequeue();
        }
    }

    private sealed class FakeEmailSmtpClient : IEmailSmtpClient
    {
        public string? Host { get; private set; }
        public int Port { get; private set; }
        public SecureSocketOptions SecureSocketOptions { get; private set; }
        public string? UserName { get; private set; }
        public string? Password { get; private set; }
        public MimeMessage? SentMessage { get; private set; }
        public Exception? SendException { get; init; }
        public int SendAttempts { get; private set; }
        public bool Disconnected { get; private set; }
        public bool Disposed { get; private set; }
        public bool InvalidServerCertificateAllowed { get; private set; }

        public void AllowInvalidServerCertificate()
        {
            InvalidServerCertificateAllowed = true;
        }

        public Task ConnectAsync(
            string host,
            int port,
            SecureSocketOptions options,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Host = host;
            Port = port;
            SecureSocketOptions = options;
            return Task.CompletedTask;
        }

        public Task AuthenticateAsync(
            string userName,
            string password,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            UserName = userName;
            Password = password;
            return Task.CompletedTask;
        }

        public Task SendAsync(
            MimeMessage message,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SendAttempts++;

            if (SendException is not null)
            {
                throw SendException;
            }

            SentMessage = message;
            return Task.CompletedTask;
        }

        public Task DisconnectAsync(
            bool quit,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Disconnected = quit;
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            Disposed = true;
            return ValueTask.CompletedTask;
        }
    }
}
