using SteamApp.Application.DTOs.AutomaticQueue;
using SteamApp.Domain.Enums;

namespace SteamApp.Interfaces.Services;

public interface IAutomaticQueueDataService
{
    Task<IReadOnlyList<AutomaticQueueDto>> GetDefinitionsAsync(string userId, CancellationToken cancellationToken);
    Task<AutomaticQueueDto?> GetDefinitionAsync(long id, string userId, CancellationToken cancellationToken);
    Task<AutomaticQueueDto> CreateDefinitionAsync(string userId, AutomaticQueueWriteDto input, CancellationToken cancellationToken);
    Task<AutomaticQueueDto> CreateGlobalDefinitionAsync(string administratorUserId, AutomaticQueueWriteDto input, CancellationToken cancellationToken);
    Task<AutomaticQueueDto> UpdateDefinitionAsync(long id, string userId, AutomaticQueueWriteDto input, CancellationToken cancellationToken);
    Task<AutomaticQueueDto> UpdateGlobalDefinitionAsync(long id, string administratorUserId, AutomaticQueueWriteDto input, CancellationToken cancellationToken);
    Task DeleteDefinitionAsync(long id, string userId, CancellationToken cancellationToken);
    Task DeleteGlobalDefinitionAsync(long id, CancellationToken cancellationToken);
    Task<AutomaticQueueDto> CloneDefinitionAsync(long id, string userId, CancellationToken cancellationToken);
    Task<AutomaticQueueRunDto> StartRunAsync(long id, string userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AutomaticQueueRunDto>> GetRunsAsync(string userId, long? queueId, int take, CancellationToken cancellationToken);
    Task<AutomaticQueueRunDto?> GetRunAsync(long id, string userId, CancellationToken cancellationToken);
    Task<AutomaticQueueRunDto> PauseRunAsync(
        long id,
        string userId,
        CancellationToken cancellationToken,
        AutomationPauseReasonEnum reason = AutomationPauseReasonEnum.UserRequested);
    Task<AutomaticQueueRunDto> ContinueRunAsync(long id, string userId, CancellationToken cancellationToken);
    Task<AutomaticQueueRunDto> CancelRunAsync(long id, string userId, CancellationToken cancellationToken);
    Task RecoverInterruptedRunsAsync(CancellationToken cancellationToken);
    Task ProcessActiveRunsAsync(CancellationToken cancellationToken);
}
