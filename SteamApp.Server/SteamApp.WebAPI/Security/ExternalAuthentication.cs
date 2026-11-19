using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace SteamApp.WebAPI.Security;

public sealed class ExternalAuthentication(
    IMemoryCache cache,
    IConfiguration configuration,
    TimeProvider timeProvider)
{
    public const string FrontendCors = "ExternalSignInFrontend";
    private const string CookieName = "__Host-SteamAppExternal";
    private const string FlowKey = "steamapp-flow";
    private const string Google = "Google";
    private const string Steam = "Steam";
    private static readonly TimeSpan FlowLifetime = TimeSpan.FromMinutes(10);

    private string FrontendOrigin { get; } = ReadOrigin(configuration);

    public string[] Providers { get; } = ReadProviders(configuration);

    public bool IsFrontendRequest(HttpRequest request) =>
        request.IsHttps && request.Headers.Origin.Count == 1 &&
        string.Equals(request.Headers.Origin[0], FrontendOrigin, StringComparison.Ordinal);

    public void Prepare(HttpContext context, string provider, string? userId)
    {
        if (ReadCookie(context) is { } previous)
        {
            cache.Remove((CookieName, previous));
        }

        var key = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var flow = new ExternalSignInFlow(provider, userId, timeProvider.GetUtcNow().Add(FlowLifetime));
        cache.Set((CookieName, key), flow, FlowLifetime);
        context.Response.Cookies.Append(CookieName, key, CookieOptions());
    }

    internal ExternalSignInFlow? GetFlow(HttpContext context)
    {
        var key = ReadCookie(context);
        return key != null &&
            cache.TryGetValue<ExternalSignInFlow>((CookieName, key), out var flow) &&
            flow != null && IsCurrent(flow)
                ? flow
                : null;
    }

    public AuthenticationProperties GetChallenge(HttpContext context) =>
        new(new Dictionary<string, string?> { [FlowKey] = ReadCookie(context) });

    internal bool IsCurrent(ExternalSignInFlow flow) => flow.ExpiresAt > timeProvider.GetUtcNow();

    internal void Consume(HttpContext context, ExternalSignInFlow flow)
    {
        flow.Consumed = true;
        flow.EmailCode = null;
        if (ReadCookie(context) is { } key)
        {
            cache.Remove((CookieName, key));
        }
        context.Response.Cookies.Delete(CookieName, CookieOptions());
    }

    public string ReturnUrl(bool success) => $"{FrontendOrigin}/login/external{(success ? "" : "?failed=1")}";

    public static void Configure(AuthenticationBuilder authentication, IConfiguration configuration)
    {
        var providers = ReadProviders(configuration);
        if (providers.Length > 0 && ReadOrigin(configuration).Length == 0)
        {
            throw new InvalidOperationException("External sign-in requires an HTTPS Authentication:External:FrontendOrigin.");
        }

        authentication.Services.AddSingleton<ExternalAuthentication>();
        authentication.Services.AddCors(options => options.AddPolicy(FrontendCors, policy =>
        {
            if (ReadOrigin(configuration) is { Length: > 0 } origin)
            {
                policy.WithOrigins(origin)
                    .WithMethods(HttpMethods.Post)
                    .WithHeaders("Content-Type", "Authorization")
                    .AllowCredentials();
            }
        }));
        if (providers.Length == 0)
        {
            return;
        }

        authentication.AddCookie(IdentityConstants.ExternalScheme);

        if (providers.Contains(Google, StringComparer.Ordinal))
        {
            authentication.AddOpenIdConnect(Google, options =>
            {
                options.Authority = "https://accounts.google.com";
                options.ClientId = configuration["Authentication:Google:ClientId"];
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
                options.CallbackPath = "/signin-google";
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.ResponseType = "code";
                options.ResponseMode = "query";
                options.UsePkce = true;
                options.MapInboundClaims = false;
                options.SaveTokens = false;
                options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Disable;
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("email");
                options.Scope.Add("profile");
                options.Events.OnTicketReceived = ReceiveTicketAsync;
                options.Events.OnRemoteFailure = HandleFailure;
            });
        }

        if (providers.Contains(Steam, StringComparer.Ordinal))
        {
            authentication.AddSteam(options =>
            {
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.CallbackPath = "/signin-steam";
                options.Events.OnTicketReceived = ReceiveTicketAsync;
                options.Events.OnRemoteFailure = HandleFailure;
            });
        }
    }

    private static async Task ReceiveTicketAsync(TicketReceivedContext ticket)
    {
        var authentication = ticket.HttpContext.RequestServices.GetRequiredService<ExternalAuthentication>();
        var flow = authentication.GetFlow(ticket.HttpContext);
        var success = false;
        if (flow != null && ticket.Properties?.Items.TryGetValue(FlowKey, out var key) == true &&
            key == ReadCookie(ticket.HttpContext) && flow.Provider == ticket.Scheme.Name)
        {
            await flow.Gate.WaitAsync(ticket.HttpContext.RequestAborted);
            try
            {
                if (flow.Started && !flow.Consumed && flow.Login == null &&
                    authentication.IsCurrent(flow) &&
                    await authentication.ReadSubjectAsync(ticket) is { } subject)
                {
                    flow.Login = new UserLoginInfo(flow.Provider, subject, flow.Provider);
                    if (flow.Provider == Google && ticket.Principal is { } principal)
                    {
                        flow.VerifiedEmail = ReadGoogleEmail(principal);
                        flow.ProfileNames = ExternalProfileNames.ReadGoogleClaims(principal);
                    }
                    success = true;
                }
            }
            finally
            {
                flow.Gate.Release();
            }
        }

        ticket.HandleResponse();
        ticket.Response.Headers.CacheControl = "no-store";
        ticket.Response.Redirect(authentication.ReturnUrl(success));
    }

    private async Task<string?> ReadSubjectAsync(TicketReceivedContext ticket)
    {
        if (ticket.Scheme.Name == Google)
        {
            var subject = ticket.Principal?.FindFirstValue("sub");
            return subject is { Length: > 0 and <= 255 } ? subject : null;
        }

        var request = ticket.Request;
        var fields = HttpMethods.IsPost(request.Method)
            ? (IEnumerable<KeyValuePair<string, StringValues>>)
                await request.ReadFormAsync(ticket.HttpContext.RequestAborted)
            : request.Query;
        return ReadSteamSubject(fields, ticket.Principal, timeProvider.GetUtcNow());
    }

    internal static string? ReadSteamSubject(
        IEnumerable<KeyValuePair<string, StringValues>> fields,
        ClaimsPrincipal? principal,
        DateTimeOffset now)
    {
        var values = fields.ToDictionary(field => field.Key, field => field.Value, StringComparer.Ordinal);
        var signed = ReadOpenIdField(values, "signed")?.Split(',');
        string[] required = ["op_endpoint", "claimed_id", "identity", "return_to", "response_nonce", "assoc_handle"];
        var claimedId = ReadOpenIdField(values, "claimed_id");
        var nonce = ReadOpenIdField(values, "response_nonce");
        if (signed == null || required.Any(name => !signed.Contains(name, StringComparer.Ordinal) ||
                ReadOpenIdField(values, name) == null) ||
            ReadOpenIdField(values, "op_endpoint") != "https://steamcommunity.com/openid/login" ||
            claimedId == null || claimedId != ReadOpenIdField(values, "identity") ||
            claimedId != principal?.FindFirstValue(ClaimTypes.NameIdentifier) ||
            nonce is not { Length: > 20 } ||
            !DateTimeOffset.TryParseExact(nonce[..20], "yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal, out var issuedAt) ||
            issuedAt < now.Subtract(FlowLifetime) || issuedAt > now.AddMinutes(1))
        {
            return null;
        }

        const string prefix = "https://steamcommunity.com/openid/id/";
        const string legacyPrefix = "http://steamcommunity.com/openid/id/";
        var id = claimedId.StartsWith(prefix, StringComparison.Ordinal) ? claimedId[prefix.Length..] :
            claimedId.StartsWith(legacyPrefix, StringComparison.Ordinal) ? claimedId[legacyPrefix.Length..] : "";
        return id.Length == 17 && id.All(char.IsAsciiDigit) ? id : null;
    }

    private static string? ReadOpenIdField(Dictionary<string, StringValues> fields, string name) =>
        fields.TryGetValue("openid." + name, out var value) && value.Count == 1 ? value[0] : null;

    internal static string? ReadGoogleEmail(ClaimsPrincipal principal)
    {
        var email = principal.FindFirstValue("email");
        var verified = bool.TryParse(principal.FindFirstValue("email_verified"), out var value) && value;
        return verified && email is { Length: > 0 and <= 256 } &&
            (email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase) ||
                !string.IsNullOrWhiteSpace(principal.FindFirstValue("hd")))
                    ? email
                    : null;
    }

    private static Task HandleFailure(RemoteFailureContext failure)
    {
        var authentication = failure.HttpContext.RequestServices.GetRequiredService<ExternalAuthentication>();
        failure.HandleResponse();
        failure.Response.Headers.CacheControl = "no-store";
        failure.Response.Redirect(authentication.ReturnUrl(false));
        return Task.CompletedTask;
    }

    private static CookieOptions CookieOptions() => new()
    {
        Path = "/",
        Secure = true,
        HttpOnly = true,
        SameSite = SameSiteMode.Lax,
        MaxAge = FlowLifetime,
        IsEssential = true
    };

    private static string? ReadCookie(HttpContext context) =>
        context.Request.Cookies[CookieName] is { Length: 64 } key ? key : null;

    private static string ReadOrigin(IConfiguration configuration)
    {
        var value = configuration["Authentication:External:FrontendOrigin"];
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
            uri.Scheme == Uri.UriSchemeHttps && uri.AbsolutePath == "/" &&
            uri.UserInfo.Length == 0 && uri.Query.Length == 0 && uri.Fragment.Length == 0
                ? uri.GetLeftPart(UriPartial.Authority)
                : "";
    }

    private static string[] ReadProviders(IConfiguration configuration)
    {
        var providers = new List<string>(2);
        if (!string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientId"]) &&
            !string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientSecret"]))
        {
            providers.Add(Google);
        }
        if (configuration.GetValue("Authentication:External:SteamEnabled", false))
        {
            providers.Add(Steam);
        }
        return [.. providers];
    }
}
