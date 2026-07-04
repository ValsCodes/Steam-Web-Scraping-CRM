using static SteamApp.WebAPI.Program;

namespace SteamApp.WebAPI.MessageBrokers
{
    public interface IMessagePublisher
    {
        Task PublishAsync(PublishMessageRequest message, CancellationToken cancellationToken);
    }
}
