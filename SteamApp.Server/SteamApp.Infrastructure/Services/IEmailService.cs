namespace SteamApp.Infrastructure.Services
{
    public interface IEmailService
    {
        Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
    }

    public sealed record EmailMessage(string To, string Subject, string HtmlBody, string? PlainTextBody = null, string? From = null);

    public sealed class EmailOptions
    {
        public string SmtpHost { get; init; } = string.Empty;
        public int SmtpPort { get; init; }
        public string Username { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string DefaultFrom { get; init; } = string.Empty;
        public bool UseSsl { get; init; }
    }
}
