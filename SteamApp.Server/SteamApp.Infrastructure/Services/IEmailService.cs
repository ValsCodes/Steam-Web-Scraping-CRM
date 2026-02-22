namespace SteamApp.Infrastructure.Services
{
    public interface IEmailService
    {
        Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
    }

    public sealed record EmailMessage(string To, string Subject, string Body);

    public sealed class EmailOptions
    {
        public string ApiToken { get; init; } = string.Empty;
        public string SandboxID { get; init; } = string.Empty;
    }
}
