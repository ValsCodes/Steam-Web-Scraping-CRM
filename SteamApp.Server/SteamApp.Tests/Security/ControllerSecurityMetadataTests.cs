using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using SteamApp.WebAPI.Controllers;
using SteamApp.WebAPI.Security;

namespace SteamApp.Tests.Security;

[TestFixture]
public sealed class ControllerSecurityMetadataTests
{
    [Test]
    public void SteamController_RequiresApiUserPolicyAndExpensiveRateLimit()
    {
        var authorize = typeof(SteamController).GetCustomAttribute<AuthorizeAttribute>();
        var rateLimit = typeof(SteamController).GetCustomAttribute<EnableRateLimitingAttribute>();

        Assert.Multiple(() =>
        {
            Assert.That(authorize?.Policy, Is.EqualTo(SecurityPolicies.ApiUser));
            Assert.That(rateLimit?.PolicyName, Is.EqualTo(SecurityPolicies.ExpensiveApiRateLimit));
        });
    }

    [Test]
    public void AuthController_UsesAuthRateLimit()
    {
        var rateLimit = typeof(AuthController).GetCustomAttribute<EnableRateLimitingAttribute>();

        Assert.That(rateLimit?.PolicyName, Is.EqualTo(SecurityPolicies.AuthRateLimit));
    }
}
