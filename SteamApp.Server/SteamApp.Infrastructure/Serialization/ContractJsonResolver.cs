using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using SteamApp.Contracts.Common;

namespace SteamApp.Infrastructure.Serialization;

public sealed class ContractJsonResolver : DefaultContractResolver
{
    public static JsonSerializerSettings PersistenceSettings { get; } = new()
    {
        ContractResolver = new ContractJsonResolver()
    };

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);
        if (member.IsDefined(typeof(StringEncodedEnumAttribute), inherit: true))
        {
            property.Converter = new StringEnumConverter();
        }

        return property;
    }
}
