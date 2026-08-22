namespace SteamApp.Infrastructure.Services;

internal interface IEmailSmtpClientFactory
{
    IEmailSmtpClient Create();
}
