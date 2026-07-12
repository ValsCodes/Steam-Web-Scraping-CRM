using MailKit.Security;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MimeKit;
using SteamApp.Infrastructure.Services;
using SteamApp.Interfaces.Services;

namespace SteamApp.IntegrationTests.External;

[TestFixture]
public sealed class EmailServiceIntegrationTests
{
    [Test]
    public async Task EmailServiceSendsExpectedSmtpMessage()
    {
        var client = new RecordingEmailSmtpClient();
        var service = new EmailService(
            Options.Create(new EmailOptions
            {
                Host = "mail.example.com",
                Port = 587,
                UserName = "notifications@example.com",
                Password = "smtp-password",
                FromAddress = "notifications@example.com",
                FromName = "SteamApp",
                UseStartTls = true
            }),
            new RecordingEmailSmtpClientFactory(client),
            new TransientRetryPolicyService(
                Options.Create(new TransientRetryPolicyOptions()),
                NullLogger<TransientRetryPolicyService>.Instance),
            NullLogger<EmailService>.Instance);

        await service.SendAsync(new EmailMessage(
            "person@example.com",
            "Price reached",
            "The item dropped."));

        var sentMessage = client.Message!;
        Assert.That(sentMessage.Body, Is.TypeOf<TextPart>());
        var sentBody = (TextPart)sentMessage.Body!;

        Assert.Multiple(() =>
        {
            Assert.That(client.Host, Is.EqualTo("mail.example.com"));
            Assert.That(client.Port, Is.EqualTo(587));
            Assert.That(client.SecureSocketOptions, Is.EqualTo(SecureSocketOptions.StartTls));
            Assert.That(client.UserName, Is.EqualTo("notifications@example.com"));
            Assert.That(client.Password, Is.EqualTo("smtp-password"));
            Assert.That(sentMessage.From.Mailboxes.Single().Address, Is.EqualTo("notifications@example.com"));
            Assert.That(sentMessage.To.Mailboxes.Single().Address, Is.EqualTo("person@example.com"));
            Assert.That(sentMessage.Subject, Is.EqualTo("Price reached"));
            Assert.That(sentBody.Text, Is.EqualTo("The item dropped."));
            Assert.That(client.Disconnected, Is.True);
        });
    }

    [Test]
    public void EmailServiceRethrowsCallerCancellation()
    {
        var service = new EmailService(
            Options.Create(new EmailOptions
            {
                Host = "mail.example.com",
                Port = 587,
                UserName = "notifications@example.com",
                Password = "smtp-password",
                FromAddress = "notifications@example.com",
                FromName = "SteamApp",
                UseStartTls = true
            }),
            new RecordingEmailSmtpClientFactory(new RecordingEmailSmtpClient()),
            new TransientRetryPolicyService(
                Options.Create(new TransientRetryPolicyOptions()),
                NullLogger<TransientRetryPolicyService>.Instance),
            NullLogger<EmailService>.Instance);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.That(
            async () => await service.SendAsync(
                new EmailMessage("person@example.com", "Subject", "Body"),
                cts.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }

    private sealed class RecordingEmailSmtpClientFactory(
        RecordingEmailSmtpClient client) : IEmailSmtpClientFactory
    {
        public IEmailSmtpClient Create()
        {
            return client;
        }
    }

    private sealed class RecordingEmailSmtpClient : IEmailSmtpClient
    {
        public string? Host { get; private set; }
        public int Port { get; private set; }
        public SecureSocketOptions SecureSocketOptions { get; private set; }
        public string? UserName { get; private set; }
        public string? Password { get; private set; }
        public MimeMessage? Message { get; private set; }
        public bool Disconnected { get; private set; }

        public void AllowInvalidServerCertificate()
        {
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
            Message = message;
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
            return ValueTask.CompletedTask;
        }
    }
}
