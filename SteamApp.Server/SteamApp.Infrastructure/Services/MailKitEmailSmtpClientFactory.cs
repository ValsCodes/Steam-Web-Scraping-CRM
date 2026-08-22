namespace SteamApp.Infrastructure.Services;

internal sealed class MailKitEmailSmtpClientFactory : IEmailSmtpClientFactory
{
    public IEmailSmtpClient Create()
    {
        return new MailKitEmailSmtpClient();
    }
}
