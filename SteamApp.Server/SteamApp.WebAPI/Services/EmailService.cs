using Mailtrap;
using Mailtrap.Emails.Requests;
using Mailtrap.Emails.Responses;
using Microsoft.Extensions.Options;
using SteamApp.Infrastructure.Services;

namespace SteamApp.WebAPI.Services;

public class EmailService(IOptions<EmailOptions> options) : IEmailService
{
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            var apiToken = options.Value.ApiToken;
            var sandboxId = int.Parse(options.Value.SandboxID);

            using var mailtrapClientFactory = new MailtrapClientFactory(apiToken);

            IMailtrapClient mailtrapClient = mailtrapClientFactory.CreateClient();

            SendEmailRequest request = SendEmailRequest
                .Create()
                .From("steam-app@example.com")
                .To(message.To)
                .Subject(message.Subject)
                .Category("Integration Test")
                .Text(message.Body);
            SendEmailResponse? response = await mailtrapClient
                .Test(sandboxId)
                .Send(request);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while sending email: {0}", ex);
        }
    }
}
