using System.Diagnostics;
using OpenTelemetry;

namespace SteamApp.WebAPI.Observability;

internal sealed class TelemetryPrivacyProcessor : BaseProcessor<Activity>
{
    private static readonly string[] SensitiveAttributeNames =
    [
        "db.statement",
        "db.query.text",
        "http.target",
        "http.url",
        "url.full",
        "url.path",
        "url.query"
    ];

    public override void OnEnd(Activity activity)
    {
        foreach (var attributeName in SensitiveAttributeNames)
        {
            activity.SetTag(attributeName, null);
        }
    }
}
