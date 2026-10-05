using OpenTelemetry;
using OpenTelemetry.Logs;

namespace SteamApp.WebAPI.Observability;

internal sealed class TelemetryLogPrivacyProcessor : BaseProcessor<LogRecord>
{
    private static readonly string[] SensitiveAttributeFragments =
    [
        "account",
        "authorization",
        "body",
        "cachekey",
        "connectionstring",
        "credential",
        "email",
        "endpoint",
        "header",
        "jwt",
        "password",
        "payload",
        "rediskey",
        "secret",
        "token",
        "uri",
        "url",
        "user",
        "wishlistid"
    ];

    public override void OnEnd(LogRecord logRecord)
    {
        logRecord.FormattedMessage = null;
        logRecord.Exception = null;

        if (logRecord.Attributes is null)
        {
            return;
        }

        logRecord.Attributes = logRecord.Attributes
            .Where(attribute => !IsSensitive(attribute.Key))
            .ToList();
    }

    private static bool IsSensitive(string attributeName)
    {
        return SensitiveAttributeFragments.Any(fragment =>
            attributeName.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    }
}
