using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SteamApp.Interfaces.Services;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SteamApp.Infrastructure.Services;

public class EmailService : IEmailService
{
    private static readonly HttpClient SharedHttpClient = new()
    {
        BaseAddress = new Uri("https://sandbox.api.mailtrap.io/")
    };

    private readonly IOptions<EmailOptions> options;
    private readonly HttpClient httpClient;
    private readonly ITransientRetryPolicyService retryPolicy;

    public EmailService(IOptions<EmailOptions> options)
        : this(options, SharedHttpClient, CreateDefaultRetryPolicy())
    {
    }

    public EmailService(
        IOptions<EmailOptions> options,
        ITransientRetryPolicyService retryPolicy)
        : this(options, SharedHttpClient, retryPolicy)
    {
    }

    internal EmailService(IOptions<EmailOptions> options, HttpClient httpClient)
        : this(options, httpClient, CreateDefaultRetryPolicy())
    {
    }

    internal EmailService(
        IOptions<EmailOptions> options,
        HttpClient httpClient,
        ITransientRetryPolicyService retryPolicy)
    {
        this.options = options;
        this.httpClient = httpClient;
        this.retryPolicy = retryPolicy;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            var apiToken = options.Value.ApiToken;
            var sandboxId = long.Parse(options.Value.SandboxID);

            await retryPolicy.ExecuteAsync(
                "Send email",
                async ct =>
                {
                    using var request = new HttpRequestMessage(HttpMethod.Post, $"api/send/{sandboxId}");

                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
                    request.Content = JsonContent.Create(
                        new MailtrapSendRequest(
                            From: new MailtrapEmailAddress("steam-app@example.com"),
                            To: [new MailtrapEmailAddress(message.To)],
                            Subject: message.Subject,
                            Category: "Integration Test",
                            Text: message.Body));

                    using var response = await httpClient.SendAsync(request, ct);
                    response.EnsureSuccessStatusCode();
                },
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while sending email: {0}", ex);
        }
    }

    private sealed record MailtrapSendRequest(
        [property: JsonPropertyName("from")] MailtrapEmailAddress From,
        [property: JsonPropertyName("to")] MailtrapEmailAddress[] To,
        [property: JsonPropertyName("subject")] string Subject,
        [property: JsonPropertyName("category")] string Category,
        [property: JsonPropertyName("text")] string Text);

    private sealed record MailtrapEmailAddress(
        [property: JsonPropertyName("email")] string Email);

    private static ITransientRetryPolicyService CreateDefaultRetryPolicy()
    {
        return new TransientRetryPolicyService(
            Options.Create(new TransientRetryPolicyOptions()),
            NullLogger<TransientRetryPolicyService>.Instance);
    }
}
