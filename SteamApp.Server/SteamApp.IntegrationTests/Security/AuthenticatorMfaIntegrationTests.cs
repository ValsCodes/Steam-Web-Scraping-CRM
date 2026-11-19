using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OtpNet;
using SteamApp.Infrastructure.Persistence;
using SteamApp.Infrastructure.Identity;
using SteamApp.IntegrationTests.Support;
using SteamApp.WebAPI.Security;

namespace SteamApp.IntegrationTests.Security;

[TestFixture]
public sealed class AuthenticatorMfaIntegrationTests
{
    private const string Mfa = "/api/auth/profile/mfa";
    private const string Origin = "https://spa.example.test";

    [Test]
    public async Task ProfileNavigation_PreservesPendingSetupAndLeavesRoomToConfirmIt()
    {
        using var factory = new SteamAppFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateAnonymousClient();
        SetToken(client, await ReadTokenAsync(await LoginAsync(client)));

        var initial = await client.PostAsJsonAsync(Mfa + "/setup", new { });
        Assert.That(initial.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var setup = await initial.ReadJsonElementAsync();

        for (var visit = 0; visit < 3; visit++)
        {
            foreach (var path in new[] { "/api/auth/profile", "/api/auth/profile/sign-in-methods", Mfa, "/api/auth/external/providers" })
            {
                using var response = await client.GetAsync(path);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), path);
            }
        }

        var resumed = await client.PostAsJsonAsync(Mfa + "/setup", new { });
        Assert.That(resumed.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var resumedSetup = await resumed.ReadJsonElementAsync();
        Assert.Multiple(() =>
        {
            Assert.That(resumedSetup.GetProperty("setupId").GetString(), Is.EqualTo(setup.GetProperty("setupId").GetString()));
            Assert.That(resumedSetup.GetProperty("expiresAtUtc").GetString(), Is.EqualTo(setup.GetProperty("expiresAtUtc").GetString()));
        });

        var enabled = await client.PostAsJsonAsync(Mfa + "/enable", EnrollmentRequest(setup));
        Assert.That(enabled.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task PasswordSignIn_RequiresFactorAndRevokesSessionsWhenMfaChanges()
    {
        using var factory = new SteamAppFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateAnonymousClient();
        var (oldToken, key, codes) = await EnrollAsync(client);
        using var stale = factory.CreateAnonymousClient();
        SetToken(stale, oldToken);
        Assert.That((await stale.GetAsync(Mfa)).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

        var challenge = await ChallengeAsync(await LoginAsync(client));
        var later = DateTime.UtcNow.AddSeconds(30);
        var code = new Totp(Base32Encoding.ToBytes(key)).ComputeTotp(later);
        var verified = await VerifyAsync(client, challenge, "authenticator", code);
        Assert.That(verified.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var signIn = await ReadTokenAsync(verified);
        SetToken(client, signIn);

        var renewal = await client.PostAsJsonAsync("/api/auth/renew", new { });
        Assert.That(renewal.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var renewed = await ReadTokenAsync(renewal);
        var jwt = new JwtSecurityTokenHandler();
        Assert.Multiple(() =>
        {
            Assert.That(Claim(renewed, UserSessionClaims.Mfa), Is.EqualTo("true"));
            Assert.That(Claim(renewed, "auth_time"), Is.EqualTo(Claim(signIn, "auth_time")));
            Assert.That(jwt.ReadJwtToken(renewed).ValidTo, Is.LessThanOrEqualTo(jwt.ReadJwtToken(signIn).ValidTo));
        });

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var stored = await db.Set<IdentityUserToken<string>>().SingleAsync();
        Assert.That(stored.Value, Does.Not.Contain(key).And.Not.Contain(codes[0]));

        var disable = await client.DeleteAsync(Mfa);
        Assert.That(disable.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var disabledSession = await ReadTokenAsync(disable);
        Assert.That((await client.GetAsync(Mfa)).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        SetToken(client, disabledSession);
        var status = await client.GetAsync(Mfa);
        Assert.That((await status.ReadJsonElementAsync()).GetProperty("enabled").GetBoolean(), Is.False);
        Assert.That((await LoginAsync(client)).StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [TestCase("recovery", false)]
    [TestCase("recovery", true)]
    [TestCase("authenticator", false)]
    public async Task SecondFactor_ConcurrentUse_IssuesOneToken(string method, bool sameChallenge)
    {
        using var factory = new SteamAppFactory(sharedDatabase: Guid.NewGuid().ToString("N"));
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateAnonymousClient();
        using var other = factory.CreateAnonymousClient();
        var (_, key, codes) = await EnrollAsync(client);
        var challenge = await ChallengeAsync(await LoginAsync(other));
        var second = sameChallenge ? challenge : await ChallengeAsync(await LoginAsync(other));
        var code = method == "recovery"
            ? codes[0]
            : new Totp(Base32Encoding.ToBytes(key)).ComputeTotp(DateTime.UtcNow.AddSeconds(30));

        var attempts = await Task.WhenAll(
            VerifyAsync(other, challenge, method, code),
            VerifyAsync(other, second, method, code));

        Assert.That(attempts.Select(response => response.StatusCode), Is.EquivalentTo(new[]
        {
            HttpStatusCode.OK,
            sameChallenge ? HttpStatusCode.Gone : HttpStatusCode.BadRequest
        }));
        var status = await client.GetAsync(Mfa);
        Assert.That((await status.ReadJsonElementAsync()).GetProperty("recoveryCodesRemaining").GetInt32(),
            Is.EqualTo(method == "recovery" ? 9 : 10));
    }

    [Test]
    public async Task IncorrectFactorCodes_AcrossPasswordChallenges_LockTheAccount()
    {
        using var factory = new SteamAppFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateAnonymousClient();
        await EnrollAsync(client);
        var first = await ChallengeAsync(await LoginAsync(client));
        var second = await ChallengeAsync(await LoginAsync(client));

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var response = await VerifyAsync(client, attempt % 2 == 0 ? first : second, "recovery", "wrong");
            Assert.That(response.StatusCode, Is.EqualTo(attempt == 4 ? HttpStatusCode.Gone : HttpStatusCode.BadRequest));
        }

        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        Assert.That(await users.IsLockedOutAsync((await users.FindByIdAsync(IntegrationSeed.UserId))!), Is.True);
    }

    [Test]
    public async Task ReplaceRecoveryCodes_InvalidatesThePreviousSet()
    {
        using var factory = new SteamAppFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateAnonymousClient();
        var (_, _, oldCodes) = await EnrollAsync(client);

        var replacement = await client.PostAsJsonAsync(Mfa + "/recovery-codes", new { });
        Assert.That(replacement.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var newCodes = (await replacement.ReadJsonElementAsync()).GetProperty("recoveryCodes").Deserialize<string[]>()!;
        Assert.That(newCodes, Has.Length.EqualTo(10));

        var challenge = await ChallengeAsync(await LoginAsync(client));
        Assert.That((await VerifyAsync(client, challenge, "recovery", oldCodes[0])).StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That((await VerifyAsync(client, challenge, "recovery", newCodes[0])).StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task SetupMfa_OlderSession_RequiresFreshSignIn()
    {
        using var factory = new SteamAppFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateAuthenticatedClient(authenticationTimeUtc: DateTime.UtcNow.AddMinutes(-6));

        Assert.That((await client.GetAsync(Mfa)).StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var setup = await client.PostAsJsonAsync(Mfa + "/setup", new { });
        Assert.That(setup.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That((await setup.ReadJsonElementAsync()).GetProperty("type").GetString(),
            Is.EqualTo("urn:steamapp:mfa:recent-sign-in-required"));
    }

    [Test]
    public async Task CompleteProvider_NewAccount_IssuesTokenAndSubsequentSignInKeepsSameUser()
    {
        using var factory = new SteamAppFactory(overrides: new Dictionary<string, string?>
        {
            ["Authentication__External__FrontendOrigin"] = Origin
        });
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateAnonymousClient();
        var login = new UserLoginInfo("Google", "new-google-account", "Google");

        var cookie = PrepareProviderCompletion(factory, login, "new-google@gmail.com", new("Ada", "Lovelace"));
        var responses = await Task.WhenAll(PostProviderCompletionAsync(client, cookie),
            PostProviderCompletionAsync(client, cookie));
        using var first = responses[0];
        using var second = responses[1];
        Assert.That(responses.Select(response => response.StatusCode),
            Is.EquivalentTo(new[] { HttpStatusCode.OK, HttpStatusCode.Gone }));
        var signedIn = responses.Single(response => response.StatusCode == HttpStatusCode.OK);
        var firstToken = new JwtSecurityTokenHandler().ReadJwtToken(await ReadTokenAsync(signedIn));
        Assert.That(firstToken.Claims.Any(claim => claim.Type == System.Security.Claims.ClaimTypes.Role &&
            claim.Value == SecurityPolicies.UserRole), Is.True);
        Assert.That(firstToken.Claims.Single(claim => claim.Type == "given_name").Value, Is.EqualTo("Ada"));
        Assert.That(firstToken.Claims.Single(claim => claim.Type == "family_name").Value, Is.EqualTo("Lovelace"));

        using var repeat = await CompleteProviderAsync(factory, client, login);
        Assert.That(repeat.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var secondToken = new JwtSecurityTokenHandler().ReadJwtToken(await ReadTokenAsync(repeat));
        Assert.That(secondToken.Subject, Is.EqualTo(firstToken.Subject));
    }

    [Test]
    public async Task CompleteProvider_ExistingAccount_FillsOnlyMissingName()
    {
        using var factory = new SteamAppFactory(overrides: new Dictionary<string, string?>
        {
            ["Authentication__External__FrontendOrigin"] = Origin
        });
        await factory.ResetDatabaseAsync();
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = "google-user", Email = "existing@example.test", FirstName = "Edited" };
        var login = new UserLoginInfo("Google", "existing-google-account", "Google");
        Assert.That((await users.CreateAsync(user)).Succeeded, Is.True);
        Assert.That((await users.AddLoginAsync(user, login)).Succeeded, Is.True);

        using var client = factory.CreateAnonymousClient();
        var cookie = PrepareProviderCompletion(factory, login, names: new("Ada", "Lovelace"));
        using var response = await PostProviderCompletionAsync(client, cookie);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        SetToken(client, await ReadTokenAsync(response));
        using var profile = await client.GetAsync("/api/auth/profile");
        var body = await profile.ReadJsonElementAsync();

        Assert.Multiple(() =>
        {
            Assert.That(body.GetProperty("firstName").GetString(), Is.EqualTo("Edited"));
            Assert.That(body.GetProperty("lastName").GetString(), Is.EqualTo("Lovelace"));
        });
    }

    [TestCase("Google")]
    [TestCase("Steam")]
    public async Task ProviderOnlyAccount_RequiresSteamAppMfaAfterProviderSignIn(string provider)
    {
        using var factory = new SteamAppFactory(overrides: new Dictionary<string, string?>
        {
            ["Authentication__External__FrontendOrigin"] = Origin
        });
        await factory.ResetDatabaseAsync();
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = "provider-only", Email = "provider-only@example.test" };
        const string providerKey = "test-provider-account";
        var login = new UserLoginInfo(provider, providerKey, provider);
        Assert.That((await users.CreateAsync(user)).Succeeded, Is.True);
        Assert.That((await users.AddLoginAsync(user, login)).Succeeded, Is.True);

        using var client = factory.CreateAnonymousClient();
        var initial = await CompleteProviderAsync(factory, client, login);
        Assert.That(initial.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var (_, _, codes) = await EnrollAsync(client, initial);
        var cookie = PrepareProviderCompletion(factory, login,
            names: provider == "Google" ? new("Ada", "Lovelace") : null);
        var challenge = await ChallengeAsync(await PostProviderCompletionAsync(client, cookie));
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.That(await db.Users.Where(account => account.Id == user.Id)
            .Select(account => account.FirstName).SingleAsync(), Is.Null);
        var response = await VerifyAsync(client, challenge, "recovery", codes[0]);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var token = await ReadTokenAsync(response);
        Assert.Multiple(() =>
        {
            Assert.That(Claim(token, UserSessionClaims.Mfa), Is.EqualTo("true"));
            Assert.That(Claim(token, "steamapp_external_provider"), Is.EqualTo(provider));
            Assert.That(new JwtSecurityTokenHandler().ReadJwtToken(token).Subject, Is.EqualTo(user.Id));
            Assert.That(new JwtSecurityTokenHandler().ReadJwtToken(token).Claims
                .SingleOrDefault(claim => claim.Type == "given_name")?.Value,
                Is.EqualTo(provider == "Google" ? "Ada" : null));
            Assert.That(user.PasswordHash, Is.Null);
        });
    }

    [Test]
    public async Task EnableMfa_TokenWriteFails_RollsBackAndAllowsRetry()
    {
        using var factory = new SteamAppFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateAnonymousClient();
        SetToken(client, await ReadTokenAsync(await LoginAsync(client)));
        var setup = await (await client.PostAsJsonAsync(Mfa + "/setup", new { })).ReadJsonElementAsync();
        var request = EnrollmentRequest(setup);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TRIGGER fail_mfa_insert BEFORE INSERT ON AspNetUserTokens
            BEGIN SELECT RAISE(ABORT, 'test transaction failure'); END;
            """);

        var failure = await client.PostAsJsonAsync(Mfa + "/enable", request);
        Assert.That(failure.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        Assert.That((await db.Users.AsNoTracking().SingleAsync()).TwoFactorEnabled, Is.False);
        Assert.That(await db.Set<IdentityUserToken<string>>().AnyAsync(), Is.False);

        await db.Database.ExecuteSqlRawAsync("DROP TRIGGER fail_mfa_insert");
        Assert.That((await client.PostAsJsonAsync(Mfa + "/enable", request)).StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task AppRestart_DropsPendingChallengesButRetainsAuthenticatorEnrollment()
    {
        var keyRing = Directory.CreateTempSubdirectory("steamapp-mfa-keys-");
        try
        {
            var database = Guid.NewGuid().ToString("N");
            using var keeper = new SqliteConnection($"Data Source={database};Mode=Memory;Cache=Shared");
            await keeper.OpenAsync();
            var settings = new Dictionary<string, string?>
            {
                ["Authentication__DataProtectionKeyPath"] = keyRing.FullName
            };
            string key;
            string pending;
            using (var firstHost = new SteamAppFactory(overrides: settings, sharedDatabase: database))
            {
                await firstHost.ResetDatabaseAsync();
                using var firstClient = firstHost.CreateAnonymousClient();
                (_, key, _) = await EnrollAsync(firstClient);
                pending = await ChallengeAsync(await LoginAsync(firstClient));
            }

            using var restarted = new SteamAppFactory(overrides: settings, sharedDatabase: database);
            using var client = restarted.CreateAnonymousClient();
            var code = new Totp(Base32Encoding.ToBytes(key)).ComputeTotp(DateTime.UtcNow.AddSeconds(30));
            Assert.That((await VerifyAsync(client, pending, "authenticator", code)).StatusCode, Is.EqualTo(HttpStatusCode.Gone));
            var challenge = await ChallengeAsync(await LoginAsync(client));
            Assert.That((await VerifyAsync(client, challenge, "authenticator", code)).StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
        finally
        {
            keyRing.Delete(recursive: true);
        }
    }

    private static async Task<(string OldToken, string Key, string[] Codes)> EnrollAsync(
        HttpClient client, HttpResponseMessage? initialLogin = null)
    {
        var oldToken = await ReadTokenAsync(initialLogin ?? await LoginAsync(client));
        SetToken(client, oldToken);
        var setupResponse = await client.PostAsJsonAsync(Mfa + "/setup", new { });
        Assert.That(setupResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(setupResponse.Headers.CacheControl?.NoStore, Is.True);
        var setup = await setupResponse.ReadJsonElementAsync();
        var enable = await client.PostAsJsonAsync(Mfa + "/enable", EnrollmentRequest(setup));
        Assert.That(enable.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var enrolled = await enable.ReadJsonElementAsync();
        var codes = enrolled.GetProperty("recoveryCodes").Deserialize<string[]>()!;
        Assert.That(codes, Has.Length.EqualTo(10));
        Assert.That(codes, Is.Unique);
        SetToken(client, enrolled.GetProperty("session").GetProperty("token").GetString()!);
        return (oldToken, setup.GetProperty("manualKey").GetString()!, codes);
    }

    private static object EnrollmentRequest(JsonElement setup) => new
    {
        setupId = setup.GetProperty("setupId").GetString(),
        code = new Totp(Base32Encoding.ToBytes(setup.GetProperty("manualKey").GetString()!)).ComputeTotp(DateTime.UtcNow)
    };

    private static Task<HttpResponseMessage> LoginAsync(HttpClient client) =>
        client.PostAsJsonAsync("/api/auth/login", new
        {
            emailOrUserName = IntegrationSeed.UserName,
            password = IntegrationSeed.UserPassword
        });

    private static Task<HttpResponseMessage> CompleteProviderAsync(
        SteamAppFactory factory, HttpClient client, UserLoginInfo login) =>
        PostProviderCompletionAsync(client, PrepareProviderCompletion(factory, login));

    private static string PrepareProviderCompletion(
        SteamAppFactory factory, UserLoginInfo login, string? verifiedEmail = null, ExternalProfileNames? names = null)
    {
        var external = factory.Services.GetRequiredService<ExternalAuthentication>();
        var simulatedCallback = new DefaultHttpContext();
        external.Prepare(simulatedCallback, login.LoginProvider, null);
        var cookie = simulatedCallback.Response.Headers.SetCookie[0]!.Split(';')[0];
        simulatedCallback.Request.Headers.Cookie = cookie;
        var flow = external.GetFlow(simulatedCallback)!;
        flow.Login = login;
        flow.VerifiedEmail = verifiedEmail;
        flow.ProfileNames = names;
        return cookie;
    }

    private static async Task<HttpResponseMessage> PostProviderCompletionAsync(HttpClient client, string cookie)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/external/complete")
        {
            Content = JsonContent.Create(new { })
        };
        request.Headers.Add("Origin", Origin);
        request.Headers.Add("Cookie", cookie);
        return await client.SendAsync(request);
    }

    private static async Task<string> ChallengeAsync(HttpResponseMessage response)
    {
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
        Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
        var body = await response.ReadJsonElementAsync();
        Assert.That(body.TryGetProperty("token", out _), Is.False);
        Assert.That(body.GetProperty("requiresMfa").GetBoolean(), Is.True);
        return body.GetProperty("challengeId").GetString()!;
    }

    private static Task<HttpResponseMessage> VerifyAsync(HttpClient client, string challengeId, string method, string code) =>
        client.PostAsJsonAsync("/api/auth/mfa/verify", new { challengeId, method, code });

    private static async Task<string> ReadTokenAsync(HttpResponseMessage response) =>
        (await response.ReadJsonElementAsync()).GetProperty("token").GetString()!;

    private static void SetToken(HttpClient client, string token) =>
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    private static string Claim(string token, string name) =>
        new JwtSecurityTokenHandler().ReadJwtToken(token).Claims.Single(claim => claim.Type == name).Value;
}
