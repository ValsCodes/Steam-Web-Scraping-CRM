using SteamApp.Application.DTOs.ManualCheck;
using SteamApp.Domain.Enums;

namespace SteamApp.Interfaces.Services;

public interface IManualCheckDataService
{
    Task<IReadOnlyList<ManualCheckConditionOperatorDto>> GetConditionOperatorsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<ManualCheckPresetDto>> GetPresetsAsync(long? gameId, CancellationToken cancellationToken);
    Task<ManualCheckPresetDto> CreatePresetAsync(ManualCheckPresetWriteDto input, CancellationToken cancellationToken);
    Task<ManualCheckPresetDto> UpdatePresetAsync(long id, ManualCheckPresetWriteDto input, CancellationToken cancellationToken);
    Task DeletePresetAsync(long id, CancellationToken cancellationToken);
    Task<ManualCheckRunSummaryDto> CreateRunAsync(
        long gameUrlId,
        long presetId,
        bool bypassCache,
        CancellationToken cancellationToken);
    Task<ManualCheckRunSummaryDto> RerunAsync(long runId, CancellationToken cancellationToken);
    Task<ManualCheckRunDetailDto> CancelAsync(long runId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ManualCheckRunSummaryDto>> GetRunsAsync(long? gameId, int take, CancellationToken cancellationToken);
    Task<ManualCheckRunDetailDto?> GetRunAsync(long id, CancellationToken cancellationToken);
    Task<ManualCheckSetupDto?> MarkRunningAndGetSetupAsync(long id, CancellationToken cancellationToken);
    Task UpdateProgressAsync(
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
