using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MimeKit;
using SteamApp.Interfaces.Services;

namespace SteamApp.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IOptions<EmailOptions> options;
    private readonly IEmailSmtpClientFactory smtpClientFactory;
    private readonly ITransientRetryPolicyService retryPolicy;
    private readonly ILogger<EmailService> logger;

    public EmailService(IOptions<EmailOptions> options)
        : this(
            options,
            new MailKitEmailSmtpClientFactory(),
            CreateDefaultRetryPolicy(),
            NullLogger<EmailService>.Instance)
    {
    }

    public EmailService(
        IOptions<EmailOptions> options,
        ITransientRetryPolicyService retryPolicy)
        : this(
            options,
            new MailKitEmailSmtpClientFactory(),
            retryPolicy,
            NullLogger<EmailService>.Instance)
    {
    }

    public EmailService(
        IOptions<EmailOptions> options,
        ITransientRetryPolicyService retryPolicy,
        ILogger<EmailService> logger)
        : this(
            options,
            new MailKitEmailSmtpClientFactory(),
            retryPolicy,
            logger)
    {
    }

    internal EmailService(
        IOptions<EmailOptions> options,
        IEmailSmtpClientFactory smtpClientFactory,
        ITransientRetryPolicyService retryPolicy,
        ILogger<EmailService> logger)
    {
        this.options = options;
        this.smtpClientFactory = smtpClientFactory;
        this.retryPolicy = retryPolicy;
        this.logger = logger;
    }

    public async Task SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await retryPolicy.ExecuteAsync(
                "Send email",
                async ct =>
                {
                    var emailOptions = options.Value;
                    var mimeMessage = CreateMessage(emailOptions, message);
                    await using var smtpClient = smtpClientFactory.Create();

                    if (emailOptions.AllowInvalidCertificate)
                    {
                        smtpClient.AllowInvalidServerCertificate();
                    }

                    await smtpClient.ConnectAsync(
                        emailOptions.Host,
                        emailOptions.Port,
                        GetSecureSocketOptions(emailOptions),
                        ct);

                    await smtpClient.AuthenticateAsync(
                        emailOptions.UserName,
                        emailOptions.Password,
                        ct);

                    await smtpClient.SendAsync(mimeMessage, ct);
                    await smtpClient.DisconnectAsync(true, ct);
                },
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while sending email.");
        }
    }

    private static MimeMessage CreateMessage(
        EmailOptions options,
        EmailMessage message)
    {
        var mimeMessage = new MimeMessage();

        mimeMessage.From.Add(new MailboxAddress(
            string.IsNullOrWhiteSpace(options.FromName)
                ? options.FromAddress
                : options.FromName,
            options.FromAddress));
        mimeMessage.To.Add(MailboxAddress.Parse(message.To));
        mimeMessage.Subject = message.Subject;
        mimeMessage.Body = new TextPart("plain")
        {
            Text = message.Body
        };

        return mimeMessage;
    }

    private static SecureSocketOptions GetSecureSocketOptions(EmailOptions options)
    {
        return options.UseStartTls
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.Auto;
    }

    private static ITransientRetryPolicyService CreateDefaultRetryPolicy()
    {
        return new TransientRetryPolicyService(
            Options.Create(new TransientRetryPolicyOptions()),
            NullLogger<TransientRetryPolicyService>.Instance);
    }
}

internal interface IEmailSmtpClientFactory
{
    IEmailSmtpClient Create();
}

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

internal sealed class MailKitEmailSmtpClientFactory : IEmailSmtpClientFactory
{
    public IEmailSmtpClient Create()
    {
        return new MailKitEmailSmtpClient();
    }
}

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

/*
Legacy Mailtrap HTTP fallback reference.

Required usings:
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

Required legacy EmailOptions fields:
public string ApiToken { get; init; } = string.Empty;
public string SandboxID { get; init; } = string.Empty;

private static readonly HttpClient SharedHttpClient = new()
{
    BaseAddress = new Uri("https://sandbox.api.mailtrap.io/")
};

private async Task SendWithMailtrapHttpAsync(
    EmailMessage message,
    CancellationToken cancellationToken)
{
    var apiToken = options.Value.ApiToken;
    var sandboxId = long.Parse(options.Value.SandboxID);

    await retryPolicy.ExecuteAsync(
        "Send email",
        async ct =>
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"api/send/{sandboxId}");

            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                apiToken);

            request.Content = JsonContent.Create(
                new MailtrapSendRequest(
                    From: new MailtrapEmailAddress("steam-app@example.com"),
                    To: [new MailtrapEmailAddress(message.To)],
                    Subject: message.Subject,
                    Category: "Integration Test",
                    Text: message.Body));

            using var response = await SharedHttpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();
        },
        cancellationToken);
}

private sealed record MailtrapSendRequest(
    [property: JsonPropertyName("from")] MailtrapEmailAddress From,
    [property: JsonPropertyName("to")] MailtrapEmailAddress[] To,
    [property: JsonPropertyName("subject")] string Subject,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("text")] string Text);

private sealed record MailtrapEmailAddress(
    [property: JsonPropertyName("email")] string Email);
*/
