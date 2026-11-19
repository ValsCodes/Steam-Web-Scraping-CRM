using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Moq;
using SteamApp.WebAPI.Security;

namespace SteamApp.Tests.Security;

[TestFixture]
public sealed class ExternalAuthenticationTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);

    [TestCase("user@gmail.com", "true", null, true)]
    [TestCase("user@example.com", "true", "example.com", true)]
    [TestCase("user@example.com", "true", null, false)]
    [TestCase("user@gmail.com", "false", null, false)]
    public void ReadGoogleEmail_OnlyAcceptsVerifiedGoogleHostedEmail(
        string email, string verified, string? hostedDomain, bool trusted)
    {
        var claims = new ClaimsIdentity([new Claim("email", email), new Claim("email_verified", verified)]);
        if (hostedDomain != null)
        {
            claims.AddClaim(new Claim("hd", hostedDomain));
        }

        Assert.That(ExternalAuthentication.ReadGoogleEmail(new ClaimsPrincipal(claims)),
            Is.EqualTo(trusted ? email : null));
    }

    [TestCase("https://steamcommunity.com/openid/login", "claimed_id", 0, true)]
    [TestCase("https://example.com/openid/login", "claimed_id", 0, false)]
    [TestCase("https://steamcommunity.com/openid/login", "unsigned", 0, false)]
    [TestCase("https://steamcommunity.com/openid/login", "claimed_id", -11, false)]
    public void ReadSteamSubject_RequiresSignedFreshSteamIdentity(
        string endpoint, string signedIdentity, int nonceAgeMinutes, bool valid)
    {
        const string steamId = "76561198000000000";
        const string claimedId = "https://steamcommunity.com/openid/id/" + steamId;
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, claimedId)]));
        var fields = new Dictionary<string, StringValues>
        {
            ["openid.op_endpoint"] = endpoint,
            ["openid.claimed_id"] = claimedId,
            ["openid.identity"] = claimedId,
            ["openid.return_to"] = "https://localhost:7443/signin-steam",
            ["openid.assoc_handle"] = "fixture",
            ["openid.response_nonce"] = $"{Now.AddMinutes(nonceAgeMinutes):yyyy-MM-dd'T'HH:mm:ss'Z'}fixture",
            ["openid.signed"] = $"op_endpoint,{signedIdentity},identity,return_to,response_nonce,assoc_handle"
        };

        Assert.That(ExternalAuthentication.ReadSteamSubject(fields, principal, Now), Is.EqualTo(valid ? steamId : null));
    }

    [Test]
    public void Flow_ExpiresAndRejectsUnconfiguredOrigin()
    {
        const string origin = "https://localhost:4300";
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var clock = new Mock<TimeProvider>();
        var now = Now;
        clock.Setup(x => x.GetUtcNow()).Returns(() => now);
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Authentication:External:FrontendOrigin"] = origin
        }).Build();
        var auth = new ExternalAuthentication(cache, config, clock.Object);
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.Headers.Origin = "https://example.com";
        Assert.That(auth.IsFrontendRequest(context.Request), Is.False);
        Assert.That(auth.ReturnUrl(true), Is.EqualTo(origin + "/login/external"));

        context.Request.Headers.Origin = origin;
        auth.Prepare(context, "Steam", null);
        context.Request.Headers.Cookie = context.Response.Headers.SetCookie[0]!.Split(';')[0];
        Assert.That(auth.IsFrontendRequest(context.Request), Is.True);
        Assert.That(auth.GetFlow(context), Is.Not.Null);

        now = Now.AddMinutes(10);
        Assert.That(auth.GetFlow(context), Is.Null);
    }
}
