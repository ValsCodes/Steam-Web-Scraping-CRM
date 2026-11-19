namespace SteamApp.Infrastructure.ExternalServices;

internal interface IEmailSmtpClientFactory
{
    IEmailSmtpClient Create();
}
