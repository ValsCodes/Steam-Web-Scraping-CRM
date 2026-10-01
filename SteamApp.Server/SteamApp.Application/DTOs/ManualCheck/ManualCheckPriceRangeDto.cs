using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SteamApp.Domain.Enums;

namespace SteamApp.Application.DTOs.ManualCheck;

public sealed class ManualCheckPriceRangeDto
{
    [JsonConverter(typeof(StringEnumConverter))]
    public ManualCheckPriceRangeModeEnum Mode { get; set; }
    public long? MinimumPriceMinorUnits { get; set; }
    public long? MaximumPriceMinorUnits { get; set; }
}
