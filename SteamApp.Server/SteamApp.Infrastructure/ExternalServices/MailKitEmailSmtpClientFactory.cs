namespace SteamApp.Infrastructure.ExternalServices;

internal sealed class MailKitEmailSmtpClientFactory : IEmailSmtpClientFactory
{
    public IEmailSmtpClient Create()
    {
        return new MailKitEmailSmtpClient();
    }
}
