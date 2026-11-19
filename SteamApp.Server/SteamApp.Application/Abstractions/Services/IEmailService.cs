namespace SteamApp.Application.Abstractions.Services
{
    public interface IEmailService
    {
        Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
        Task<bool> TrySendVerificationAsync(EmailMessage message, CancellationToken cancellationToken);
    }
}
