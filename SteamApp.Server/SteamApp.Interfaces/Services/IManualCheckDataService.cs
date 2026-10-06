using SteamApp.Application.DTOs.ManualCheck;
using SteamApp.Application.DTOs.AutomaticQueue;
using SteamApp.Domain.Enums;

namespace SteamApp.Interfaces.Services;

public interface IManualCheckDataService
{
    Task<IReadOnlyList<ManualCheckConditionOperatorDto>> GetConditionOperatorsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<ManualCheckPresetDto>> GetPresetsAsync(long? gameId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ManualCheckPresetDto>> GetPresetsAsync(string userId, long? gameId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ManualCheckPresetDto>> GetPresetsAsync(string userId, long? gameId, long? gameUrlId, CancellationToken cancellationToken);
    Task<ManualCheckPresetDto> CreatePresetAsync(ManualCheckPresetWriteDto input, CancellationToken cancellationToken);
    Task<ManualCheckPresetDto> UpdatePresetAsync(long id, ManualCheckPresetWriteDto input, CancellationToken cancellationToken);
    Task DeletePresetAsync(string userId, long id, CancellationToken cancellationToken);
    Task<ManualCheckRunSummaryDto> CreateRunAsync(
        string userId,
        long gameUrlId,
        long? presetId,
        ManualCheckPresetCombinationWriteDto? presetCombination,
        bool bypassCache,
        IReadOnlyList<long>? productIds,
        CancellationToken cancellationToken);
    Task<ManualCheckSetupDto> PrepareRunSetupAsync(
        string userId,
        long gameUrlId,
        long? presetId,
        ManualCheckPresetCombinationWriteDto? presetCombination,
        AutomaticQueuePrivateTemplateDto? privateTemplate,
        bool bypassCache,
        IReadOnlyList<long>? productIds,
        CancellationToken cancellationToken);
    Task<ManualCheckRunSummaryDto> CreateRunFromSetupAsync(
        ManualCheckSetupDto setup,
        CancellationToken cancellationToken);
    Task<ManualCheckRunSummaryDto> RerunAsync(long runId, CancellationToken cancellationToken);
    Task<ManualCheckRunDetailDto> PauseAsync(long runId, CancellationToken cancellationToken);
    Task<ManualCheckRunSummaryDto> ContinueAsync(long runId, CancellationToken cancellationToken);
    Task<ManualCheckRunDetailDto> UpdateListingLimitAsync(long runId, int listingLimit, CancellationToken cancellationToken);
    Task<ManualCheckRunDetailDto> CancelAsync(long runId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ManualCheckRunSummaryDto>> GetRunsAsync(long? gameId, int take, CancellationToken cancellationToken);
    Task<IReadOnlyList<ManualCheckRunSummaryDto>> GetRunsAsync(string userId, long? gameId, int take, CancellationToken cancellationToken);
    Task<ManualCheckRunDetailDto?> GetRunAsync(long id, CancellationToken cancellationToken);
    Task<bool> UserOwnsGameAsync(string userId, long gameId, CancellationToken cancellationToken);
    Task<bool> UserOwnsGameUrlAsync(string userId, long gameUrlId, CancellationToken cancellationToken);
    Task<bool> UserOwnsPresetAsync(string userId, long presetId, CancellationToken cancellationToken);
    Task<bool> UserOwnsRunAsync(string userId, long runId, CancellationToken cancellationToken);
    Task<bool> IsQueueOwnedRunAsync(long runId, CancellationToken cancellationToken);
    Task<ManualCheckRunDetailDto?> MarkRunningAndGetRunAsync(long id, CancellationToken cancellationToken);
    Task<ManualCheckRunStatusEnum?> MarkPausedAsync(long id, CancellationToken cancellationToken);
    Task<ManualCheckRunStatusEnum?> UpdateProgressAsync(
        long id,
        int checkedProducts,
        int matchedProducts,
        int failedProducts,
        ManualCheckRunResultsDto results,
        CancellationToken cancellationToken);
    Task CompleteAsync(long id, ManualCheckRunStatusEnum status, ManualCheckRunResultsDto results, CancellationToken cancellationToken);
    Task FailAsync(long id, string errorText, ManualCheckRunResultsDto? results, CancellationToken cancellationToken);
    Task MarkInterruptedRunsFailedAsync(CancellationToken cancellationToken);
}
