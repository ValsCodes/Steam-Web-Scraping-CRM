using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SteamApp.Domain.Entities;
using SteamApp.Domain.Enums;

namespace SteamApp.Application.DTOs.ManualCheck;

public sealed class ManualCheckCriterionDto
{
    public string? NameContains { get; set; }
    public string? ValueContains { get; set; }
}

public class ManualCheckPresetWriteDto
{
    public long GameId { get; set; }
    public string Name { get; set; } = string.Empty;

    [JsonConverter(typeof(StringEnumConverter))]
    public ManualCheckMatchModeEnum MatchMode { get; set; }

    public int ListingLimit { get; set; } = ManualCheckPreset.DefaultListingLimit;
    public List<ManualCheckCriterionDto> Criteria { get; set; } = [];
}

public sealed class ManualCheckPresetDto : ManualCheckPresetWriteDto
{
    public long Id { get; set; }
    public string? GameName { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class ManualCheckRunRequestDto
{
    public long GameUrlId { get; set; }
    public long PresetId { get; set; }
}

public sealed class ManualCheckRunAcceptedDto
{
    public long RunId { get; set; }
    public ManualCheckRunSummaryDto Run { get; set; } = null!;
}

public sealed class ManualCheckProgressDto
{
    public int TotalProducts { get; set; }
    public int CheckedProducts { get; set; }
    public int MatchedProducts { get; set; }
    public int FailedProducts { get; set; }
}

public class ManualCheckRunSummaryDto
{
    public long Id { get; set; }
    public long? PresetId { get; set; }
    public string PresetName { get; set; } = string.Empty;
    public long GameId { get; set; }
    public string? GameName { get; set; }
    public long GameUrlId { get; set; }
    public string? GameUrlName { get; set; }
    public int TotalProducts { get; set; }
    public int CheckedProducts { get; set; }
    public int MatchedProducts { get; set; }
    public int FailedProducts { get; set; }
    public ManualCheckProgressDto Progress { get; set; } = new();

    [JsonConverter(typeof(StringEnumConverter))]
    public ManualCheckRunStatusEnum Status { get; set; }

    public DateTime Date { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string? ErrorText { get; set; }
}

public sealed class ManualCheckRunDetailDto : ManualCheckRunSummaryDto
{
    public ManualCheckSetupDto Setup { get; set; } = new();
    public ManualCheckRunResultsDto Results { get; set; } = new();
}

public sealed class ManualCheckSetupDto
{
    public long? PresetId { get; set; }
    public string PresetName { get; set; } = string.Empty;
    public long GameId { get; set; }
    public string? GameName { get; set; }
    public long GameUrlId { get; set; }
    public string? GameUrlName { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public ManualCheckMatchModeEnum MatchMode { get; set; }

    public int ListingLimit { get; set; } = ManualCheckPreset.DefaultListingLimit;
    public List<ManualCheckCriterionDto> Criteria { get; set; } = [];
    public List<ManualCheckProductInputDto> Products { get; set; } = [];
    public DateTime RequestedAtUtc { get; set; }
}

public sealed class ManualCheckProductInputDto
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public long GameUrlId { get; set; }
    public string GameUrlName { get; set; } = string.Empty;
    public string FullUrl { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public int? Rating { get; set; }
}

public sealed class ManualCheckRunResultsDto
{
    public List<ManualCheckProductResultDto> Matches { get; set; } = [];
    public List<ManualCheckProductErrorDto> Errors { get; set; } = [];
}

public sealed class ManualCheckProductResultDto
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public long GameUrlId { get; set; }
    public string GameUrlName { get; set; } = string.Empty;
    public string FullUrl { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public int? Rating { get; set; }
    public List<ManualCheckAssetMatchDto> MatchedAssets { get; set; } = [];
}

public sealed class ManualCheckAssetMatchDto
{
    public string AppId { get; set; } = string.Empty;
    public string ContextId { get; set; } = string.Empty;
    public string AssetId { get; set; } = string.Empty;
    public string ClassId { get; set; } = string.Empty;
    public string InstanceId { get; set; } = string.Empty;
    public string MarketName { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public List<ManualCheckDescriptionMatchDto> Descriptions { get; set; } = [];
}

public sealed class ManualCheckDescriptionMatchDto
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public List<int> MatchedCriterionIndexes { get; set; } = [];
}

public sealed class ManualCheckProductErrorDto
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string FullUrl { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public string? ErrorType { get; set; }
    public int? HttpStatusCode { get; set; }
    public DateTime? OccurredAtUtc { get; set; }
}
