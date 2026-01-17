using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SteamApp.Infrastructure.Services;

namespace SteamApp.WebAPI.Services
{
    public class EmailService(IOptions<EmailOptions> options) : IEmailService
    {
        private readonly EmailOptions _options = options.Value;

        public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            var email = new MimeMessage();

            email.From.Add(MailboxAddress.Parse(message.From ?? _options.DefaultFrom));
            email.To.Add(MailboxAddress.Parse(message.To));
            email.Subject = message.Subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = message.HtmlBody,
                TextBody = message.PlainTextBody
            };

            email.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            await client.ConnectAsync(
                _options.SmtpHost,
                _options.SmtpPort,
                _options.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls,
                cancellationToken
            );

            await client.AuthenticateAsync(
                _options.Username,
                _options.Password,
                cancellationToken
            );

            await client.SendAsync(email, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
    }
}
