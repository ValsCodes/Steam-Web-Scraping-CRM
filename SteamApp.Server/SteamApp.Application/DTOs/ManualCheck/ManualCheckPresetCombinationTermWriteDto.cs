using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SteamApp.Domain.Enums;

namespace SteamApp.Application.DTOs.ManualCheck;

public class ManualCheckPresetCombinationTermWriteDto
{
    public long PresetId { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public ManualCheckPresetCombinationOperatorEnum? Operator { get; set; }
}
