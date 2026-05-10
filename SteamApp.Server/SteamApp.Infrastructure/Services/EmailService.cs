using Microsoft.Extensions.Options;
using SteamApp.Interfaces.Services;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SteamApp.Infrastructure.Services;

public class EmailService(IOptions<EmailOptions> options) : IEmailService
{
    private static readonly HttpClient HttpClient = new()
    {
        BaseAddress = new Uri("https://sandbox.api.mailtrap.io/")
    };

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            var apiToken = options.Value.ApiToken;
            var sandboxId = long.Parse(options.Value.SandboxID);

            using var request = new HttpRequestMessage(HttpMethod.Post, $"api/send/{sandboxId}");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
            request.Content = JsonContent.Create(
                new MailtrapSendRequest(
                    From: new MailtrapEmailAddress("steam-app@example.com"),
                    To: [new MailtrapEmailAddress(message.To)],
                    Subject: message.Subject,
                    Category: "Integration Test",
                    Text: message.Body));

            using var response = await HttpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
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
}
