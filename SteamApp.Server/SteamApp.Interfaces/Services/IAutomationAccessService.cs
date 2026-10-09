using SteamApp.Application.DTOs.Automation;

namespace SteamApp.Interfaces.Services;

public interface IAutomationAccessService
{
    Task<AutomationUsageDto> GetUsageAsync(string userId, CancellationToken cancellationToken);
    Task EnsureCanExecuteAsync(string userId, CancellationToken cancellationToken);
    Task UpsertPresenceAsync(string userId, Guid tabId, CancellationToken cancellationToken);
    Task ReleasePresenceAsync(string userId, Guid tabId, CancellationToken cancellationToken);
    Task<AutomationPolicyDto> GetPolicyAsync(CancellationToken cancellationToken);
    Task<AutomationPolicyDto> UpdatePolicyAsync(string administratorUserId, AutomationPolicyUpdateDto input, CancellationToken cancellationToken);
    Task<AutomationPolicyDto> ResetUsageAsync(string administratorUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminAutomationUsageDto>> GetAllUsageAsync(CancellationToken cancellationToken);
}
