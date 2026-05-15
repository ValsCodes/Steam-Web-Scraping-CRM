namespace SteamApp.WebAPI.Security;

public static class SecurityPolicies
{
    public const string ApiUser = "ApiUser";
    public const string InternalJob = "InternalJob";

    public const string AuthRateLimit = "Auth";
    public const string ApiRateLimit = "Api";
    public const string ExpensiveApiRateLimit = "ExpensiveApi";

    public const string UserScope = "user";
    public const string InternalScope = "internal";
}
