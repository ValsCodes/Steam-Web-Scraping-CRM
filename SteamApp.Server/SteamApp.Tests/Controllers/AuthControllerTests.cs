using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using SteamApp.Contracts.Authentication;
using SteamApp.Infrastructure.Identity;
using SteamApp.Application.Abstractions.Services;
using SteamApp.WebAPI.Endpoints.Controllers;
using SteamApp.WebAPI.Security;

namespace SteamApp.Tests.Controllers;

[TestFixture]
public sealed class AuthControllerTests
{
    private const string JwtKey = "12345678901234567890123456789012";
    private static readonly DateTimeOffset DefaultNow =
        new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);

    [Test]
    public void Token_RejectsInvalidClientCredentials()
    {
        var controller = CreateController(clients:
        [
            new ClientDefinition
            {
                ClientId = "client",
                ClientSecret = "secret",
                AllowedScope = SecurityPolicies.UserScope
            }
        ]);

        var result = controller.Token(new TokenRequest
        {
            ClientId = "client",
            ClientSecret = "wrong"
        });

        Assert.That(result, Is.TypeOf<UnauthorizedObjectResult>());
    }

    [Test]
    public void Token_AcceptsHashedClientSecretAndReturnsScopedJwt()
    {
        var controller = CreateController(clients:
        [
            new ClientDefinition
            {
                ClientId = "client",
                ClientSecretHash = Sha256Hex("secret"),
                AllowedScope = SecurityPolicies.InternalScope
            }
        ]);

        var result = controller.Token(new TokenRequest
        {
            ClientId = "client",
            ClientSecret = "secret"
        });

        var ok = result as OkObjectResult;
        var response = ok?.Value as AuthResponse;
        var token = new JwtSecurityTokenHandler().ReadJwtToken(response!.Token);

        Assert.Multiple(() =>
        {
            Assert.That(ok, Is.Not.Null);
            Assert.That(token.Claims.Single(x => x.Type == "client_id").Value, Is.EqualTo("client"));
            Assert.That(token.Claims.Single(x => x.Type == "scope").Value, Is.EqualTo(SecurityPolicies.InternalScope));
            Assert.That(token.Issuer, Is.EqualTo("issuer"));
            Assert.That(token.Audiences.Single(), Is.EqualTo("audience"));
        });
    }

    [Test]
    public void Token_RejectsBlankClientSecret()
    {
        var controller = CreateController(clients:
        [
            new ClientDefinition
            {
                ClientId = "client",
                ClientSecretHash = Sha256Hex("secret"),
                AllowedScope = SecurityPolicies.UserScope
            }
        ]);

        var result = controller.Token(new TokenRequest
        {
            ClientId = "client",
            ClientSecret = ""
        });

        Assert.That(result, Is.TypeOf<UnauthorizedObjectResult>());
    }

    [Test]
    public async Task Register_ReturnsNotFoundWhenRegistrationIsDisabled()
    {
        var userManager = CreateUserManager();
        var controller = CreateController(
            userManager: userManager,
            configuration: Config(("Authentication:AllowRegistration", "false")),
            environmentName: Environments.Production);

        var result = await controller.Register(new RegisterRequest
        {
            Email = "user@example.com",
            Password = "Password1"
        });

        Assert.That(result, Is.TypeOf<NotFoundResult>());
        userManager.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Register_CreatesUserAndReturnsUserTokenWhenEnabled()
    {
        var userManager = CreateUserManager();
        ApplicationUser? createdUser = null;
        userManager
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "Password1"))
            .Callback<ApplicationUser, string>((user, _) =>
            {
                user.SecurityStamp = "test-stamp";
                createdUser = user;
            })
            .ReturnsAsync(IdentityResult.Success);
        userManager
            .Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(Array.Empty<string>());

        var controller = CreateController(
            userManager: userManager,
            environmentName: Environments.Development);

        var result = await controller.Register(new RegisterRequest
        {
            FirstName = "  Test  ",
            LastName = "  User  ",
            Email = "user@example.com",
            Phone = "  +3595550100  ",
            UserName = "  user-name  ",
            Password = "Password1"
        });

        var ok = result as OkObjectResult;
        var response = ok?.Value as AuthResponse;
        var token = new JwtSecurityTokenHandler().ReadJwtToken(response!.Token);

        Assert.Multiple(() =>
        {
            Assert.That(ok, Is.Not.Null);
            Assert.That(createdUser?.FirstName, Is.EqualTo("Test"));
            Assert.That(createdUser?.LastName, Is.EqualTo("User"));
            Assert.That(createdUser?.UserName, Is.EqualTo("user-name"));
            Assert.That(createdUser?.Email, Is.EqualTo("user@example.com"));
            Assert.That(createdUser?.PhoneNumber, Is.EqualTo("+3595550100"));
            Assert.That(token.Claims.Any(x => x.Type == "scope" && x.Value == SecurityPolicies.UserScope), Is.True);
            Assert.That(token.Claims.Any(x => x.Type == ClaimTypes.Role && x.Value == SecurityPolicies.UserRole), Is.True);
            Assert.That(token.Claims.Any(x => x.Type == "given_name" && x.Value == "Test"), Is.True);
            Assert.That(token.Claims.Any(x => x.Type == "family_name" && x.Value == "User"), Is.True);
            Assert.That(
                token.Claims.Single(x => x.Type == "auth_time").Value,
                Is.EqualTo(DefaultNow.ToUnixTimeSeconds().ToString()));
            Assert.That(token.ValidTo, Is.EqualTo(DefaultNow.AddMinutes(60).UtcDateTime));
        });
        userManager.Verify(
            x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), SecurityPolicies.UserRole),
            Times.Once);
    }

    [Test]
    public async Task Register_ReturnsValidationProblemForIdentityErrors()
    {
        var userManager = CreateUserManager();
        userManager
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Code = "DuplicateEmail",
                Description = "Email exists."
            }));

        var controller = CreateController(
            userManager: userManager,
            environmentName: Environments.Development);

        var result = await controller.Register(new RegisterRequest
        {
            Email = "user@example.com",
            Password = "Password1"
        });

        Assert.That(result, Is.TypeOf<ObjectResult>());
        Assert.That(((ObjectResult)result).Value, Is.TypeOf<ValidationProblemDetails>());
    }

    [Test]
    public async Task Login_WorksByEmailAndReturnsUserClaimsAndRoles()
    {
        var user = new ApplicationUser
        {
            Id = "user-id",
            SecurityStamp = "test-stamp",
            UserName = "tester",
            Email = "user@example.com"
        };
        var userManager = CreateUserManager();
        userManager.Setup(x => x.FindByEmailAsync("user@example.com")).ReturnsAsync(user);
        userManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(["Admin"]);

        var signInManager = CreateSignInManager(userManager);
        signInManager
            .Setup(x => x.CheckPasswordSignInAsync(user, "Password1", true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        var controller = CreateController(userManager: userManager, signInManager: signInManager);

        var result = await controller.Login(new LoginRequest
        {
            EmailOrUserName = "user@example.com",
            Password = "Password1"
        });

        var ok = result as OkObjectResult;
        var response = ok?.Value as AuthResponse;
        var token = new JwtSecurityTokenHandler().ReadJwtToken(response!.Token);

        Assert.Multiple(() =>
        {
            Assert.That(ok, Is.Not.Null);
            Assert.That(token.Claims.Any(x => x.Type == JwtRegisteredClaimNames.Sub && x.Value == "user-id"), Is.True);
            Assert.That(token.Claims.Any(x => x.Type == JwtRegisteredClaimNames.Jti), Is.True);
            Assert.That(token.Claims.Any(x => x.Type == ClaimTypes.NameIdentifier && x.Value == "user-id"), Is.True);
            Assert.That(token.Claims.Any(x => x.Type == ClaimTypes.Name && x.Value == "tester"), Is.True);
            Assert.That(token.Claims.Any(x => x.Type == ClaimTypes.Email && x.Value == "user@example.com"), Is.True);
            Assert.That(token.Claims.Any(x => x.Type == ClaimTypes.Role && x.Value == "Admin"), Is.True);
            Assert.That(token.Claims.Any(x => x.Type == "scope" && x.Value == SecurityPolicies.UserScope), Is.True);
            Assert.That(
                token.Claims.Single(x => x.Type == "auth_time").Value,
                Is.EqualTo(DefaultNow.ToUnixTimeSeconds().ToString()));
        });
    }

    [Test]
    public async Task Login_ConfiguredAdminUserIdGetsAdminRole()
    {
        var user = new ApplicationUser
        {
            Id = "admin-id",
            SecurityStamp = "test-stamp",
            UserName = "owner",
            Email = "OWNER@example.com"
        };
        var userManager = CreateUserManager();
        userManager.Setup(x => x.FindByEmailAsync("owner@example.com")).ReturnsAsync(user);
        userManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(Array.Empty<string>());

        var signInManager = CreateSignInManager(userManager);
        signInManager
            .Setup(x => x.CheckPasswordSignInAsync(user, "Password1", true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        var controller = CreateController(
            userManager: userManager,
            signInManager: signInManager,
            configuration: Config(("Authentication:AdminUserIds:0", "admin-id")));

        var result = await controller.Login(new LoginRequest
        {
            EmailOrUserName = "owner@example.com",
            Password = "Password1"
        });

        var ok = result as OkObjectResult;
        var response = ok?.Value as AuthResponse;
        var token = new JwtSecurityTokenHandler().ReadJwtToken(response!.Token);

        Assert.Multiple(() =>
        {
            Assert.That(ok, Is.Not.Null);
            Assert.That(token.Claims.Any(x => x.Type == ClaimTypes.Role && x.Value == SecurityPolicies.UserRole), Is.True);
            Assert.That(token.Claims.Any(x => x.Type == ClaimTypes.Role && x.Value == SecurityPolicies.AdminRole), Is.True);
        });
        userManager.Verify(x => x.AddToRoleAsync(user, SecurityPolicies.UserRole), Times.Once);
        userManager.Verify(x => x.AddToRoleAsync(user, SecurityPolicies.AdminRole), Times.Once);
    }

    [Test]
    public async Task Login_FallsBackToUserNameLookup()
    {
        var user = new ApplicationUser { Id = "user-id", UserName = "tester", SecurityStamp = "test-stamp" };
        var userManager = CreateUserManager();
        userManager.Setup(x => x.FindByEmailAsync("tester")).ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(x => x.FindByNameAsync("tester")).ReturnsAsync(user);
        userManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(Array.Empty<string>());

        var signInManager = CreateSignInManager(userManager);
        signInManager
            .Setup(x => x.CheckPasswordSignInAsync(user, "Password1", true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        var controller = CreateController(userManager: userManager, signInManager: signInManager);

        var result = await controller.Login(new LoginRequest
        {
            EmailOrUserName = "tester",
            Password = "Password1"
        });

        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task Login_RejectsMissingUser()
    {
        var controller = CreateController();

        var result = await controller.Login(new LoginRequest
        {
            EmailOrUserName = "missing",
            Password = "Password1"
        });

        Assert.That(result, Is.TypeOf<UnauthorizedObjectResult>());
    }

    [Test]
    public async Task Login_RejectsWrongPassword()
    {
        var user = new ApplicationUser { Id = "user-id", UserName = "tester" };
        var userManager = CreateUserManager();
        userManager.Setup(x => x.FindByEmailAsync("tester")).ReturnsAsync(user);

        var signInManager = CreateSignInManager(userManager);
        signInManager
            .Setup(x => x.CheckPasswordSignInAsync(user, "bad", true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        var controller = CreateController(userManager: userManager, signInManager: signInManager);

        var result = await controller.Login(new LoginRequest
        {
            EmailOrUserName = "tester",
            Password = "bad"
        });

        Assert.That(result, Is.TypeOf<UnauthorizedObjectResult>());
    }

    [Test]
    public async Task Login_RejectsLockedOutUser()
    {
        var user = new ApplicationUser { Id = "user-id", UserName = "tester" };
        var userManager = CreateUserManager();
        userManager.Setup(x => x.FindByEmailAsync("tester")).ReturnsAsync(user);

        var signInManager = CreateSignInManager(userManager);
        signInManager
            .Setup(x => x.CheckPasswordSignInAsync(user, "Password1", true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);

        var controller = CreateController(userManager: userManager, signInManager: signInManager);

        var result = await controller.Login(new LoginRequest
        {
            EmailOrUserName = "tester",
            Password = "Password1"
        });

        Assert.That(result, Is.TypeOf<UnauthorizedObjectResult>());
    }

    [Test]
    public async Task Renew_ValidUserSession_PreservesAuthenticationTimeAndCapsExpiration()
    {
        var authenticationTime = DefaultNow.AddMinutes(-55);
        var user = new ApplicationUser
        {
            Id = "user-id",
            UserName = "updated-user",
            SecurityStamp = "test-stamp",
            Email = "updated@example.com"
        };
        var userManager = CreateUserManager();
        userManager.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(user);
        userManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync([SecurityPolicies.AdminRole]);

        var controller = CreateController(userManager: userManager);
        Authenticate(controller, authenticationTime: authenticationTime);

        var result = await controller.Renew();

        var ok = result as OkObjectResult;
        var response = ok?.Value as AuthResponse;
        var token = new JwtSecurityTokenHandler().ReadJwtToken(response!.Token);

        Assert.Multiple(() =>
        {
            Assert.That(ok, Is.Not.Null);
            Assert.That(
                token.Claims.Single(x => x.Type == "auth_time").Value,
                Is.EqualTo(authenticationTime.ToUnixTimeSeconds().ToString()));
            Assert.That(
                token.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Iat).Value,
                Is.EqualTo(DefaultNow.ToUnixTimeSeconds().ToString()));
            Assert.That(token.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value, Is.Not.Empty);
            Assert.That(token.ValidTo, Is.EqualTo(authenticationTime.AddHours(1).UtcDateTime));
            Assert.That(token.Claims.Any(x => x.Type == ClaimTypes.Name && x.Value == "updated-user"), Is.True);
            Assert.That(token.Claims.Any(x => x.Type == ClaimTypes.Role && x.Value == SecurityPolicies.AdminRole), Is.True);
        });
    }

    [Test]
    public async Task Renew_LegacyOrInvalidSession_RejectsRenewal()
    {
        var legacyController = CreateController();
        Authenticate(legacyController);

        var invalidController = CreateController();
        Authenticate(invalidController, authenticationTimeClaim: "invalid");

        var expiredController = CreateController();
        Authenticate(expiredController, authenticationTime: DefaultNow.AddHours(-1));

        var legacyResult = await legacyController.Renew();
        var invalidResult = await invalidController.Renew();
        var expiredResult = await expiredController.Renew();

        Assert.Multiple(() =>
        {
            Assert.That(legacyResult, Is.TypeOf<UnauthorizedObjectResult>());
            Assert.That(invalidResult, Is.TypeOf<UnauthorizedObjectResult>());
            Assert.That(expiredResult, Is.TypeOf<UnauthorizedObjectResult>());
        });
    }

    [Test]
    public async Task Renew_MissingCurrentUser_RejectsRenewal()
    {
        var controller = CreateController();
        Authenticate(controller, authenticationTime: DefaultNow.AddMinutes(-30));

        var result = await controller.Renew();

        Assert.That(result, Is.TypeOf<UnauthorizedObjectResult>());
    }

    [Test]
    public async Task GetProfile_ReturnsCurrentUserProfile()
    {
        var user = new ApplicationUser
        {
            Id = "user-id",
            FirstName = "Test",
            LastName = "User",
            UserName = "tester",
            Email = "user@example.com",
            PhoneNumber = "+3595550100"
        };
        var userManager = CreateUserManager();
        userManager.Setup(x => x.FindByIdAsync("user-id")).ReturnsAsync(user);

        var controller = CreateController(userManager: userManager);
        Authenticate(controller);

        var result = await controller.GetProfile();

        var ok = result as OkObjectResult;
        var response = ok?.Value as UserProfileResponse;

        Assert.Multiple(() =>
        {
            Assert.That(ok, Is.Not.Null);
            Assert.That(response?.Id, Is.EqualTo("user-id"));
            Assert.That(response?.FirstName, Is.EqualTo("Test"));
            Assert.That(response?.LastName, Is.EqualTo("User"));
            Assert.That(response?.UserName, Is.EqualTo("tester"));
            Assert.That(response?.Email, Is.EqualTo("user@example.com"));
            Assert.That(response?.Phone, Is.EqualTo("+3595550100"));
            Assert.That(response?.DisplayName, Is.EqualTo("Test User"));
        });
    }

    [Test]
    public async Task UpdateProfile_UpdatesCurrentUserParameters()
    {
        var user = new ApplicationUser { Id = "user-id" };
        var userManager = CreateUserManager();
        userManager.Setup(x => x.FindByIdAsync("user-id")).ReturnsAsync(user);
        userManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var controller = CreateController(userManager: userManager);
        Authenticate(controller);

        var result = await controller.UpdateProfile(new UpdateUserProfileRequest
        {
            FirstName = "  Test  ",
            LastName = "  User  ",
            UserName = "  tester  ",
            Email = "  user@example.com  ",
            Phone = "  +3595550100  "
        });

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            Assert.That(user.FirstName, Is.EqualTo("Test"));
            Assert.That(user.LastName, Is.EqualTo("User"));
            Assert.That(user.UserName, Is.EqualTo("tester"));
            Assert.That(user.Email, Is.EqualTo("user@example.com"));
            Assert.That(user.PhoneNumber, Is.EqualTo("+3595550100"));
        });
    }

    [Test]
    public async Task ChangePassword_UsesIdentityPasswordChange()
    {
        var user = new ApplicationUser { Id = "user-id" };
        var userManager = CreateUserManager();
        userManager.Setup(x => x.FindByIdAsync("user-id")).ReturnsAsync(user);
        userManager
            .Setup(x => x.ChangePasswordAsync(user, "Password1", "Password2"))
            .ReturnsAsync(IdentityResult.Success);

        var controller = CreateController(userManager: userManager);
        Authenticate(controller);

        var result = await controller.ChangePassword(new ChangePasswordRequest
        {
            CurrentPassword = "Password1",
            NewPassword = "Password2"
        });

        Assert.That(result, Is.TypeOf<NoContentResult>());
    }

    [Test]
    public async Task DeleteProfile_RequiresPasswordAndDeletesCurrentUser()
    {
        var user = new ApplicationUser { Id = "user-id" };
        var userManager = CreateUserManager();
        userManager.Setup(x => x.FindByIdAsync("user-id")).ReturnsAsync(user);
        userManager.Setup(x => x.CheckPasswordAsync(user, "Password1")).ReturnsAsync(true);
        userManager.Setup(x => x.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

        var controller = CreateController(userManager: userManager);
        Authenticate(controller);

        var result = await controller.DeleteProfile(new DeleteUserRequest
        {
            Password = "Password1"
        });

        Assert.That(result, Is.TypeOf<NoContentResult>());
        userManager.Verify(x => x.DeleteAsync(user), Times.Once);
    }

    [Test]
    public async Task CompleteExternalSignIn_LinkedAccountGetsJwtOnceEvenWhenRegistrationIsClosed()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var users = CreateUserManager();
        var signIn = CreateSignInManager(users);
        var user = new ApplicationUser { Id = "linked", Email = "owner@example.com", SecurityStamp = "test-stamp" };
        users.Setup(x => x.FindByLoginAsync("Steam", "76561198000000000")).ReturnsAsync(user);
        signIn.Setup(x => x.CanSignInAsync(user)).ReturnsAsync(true);
        var auth = CreateController(userManager: users, signInManager: signIn,
            configuration: Config(("Authentication:AdminEmails:0", "owner@example.com")));
        var external = StartCompletedFlow(auth, cache);

        var success = await CompleteExternalAsync(auth, external);
        var replay = await CompleteExternalAsync(auth, external);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(((AuthResponse)((OkObjectResult)success).Value!).Token);
        Assert.Multiple(() =>
        {
            Assert.That(jwt.Subject, Is.EqualTo(user.Id));
            Assert.That(jwt.Claims.Any(claim => claim.Value == SecurityPolicies.AdminRole), Is.False);
            Assert.That(jwt.Claims.Any(claim => claim.Type == "steamapp_external_provider" && claim.Value == "Steam"), Is.True);
            Assert.That(((ObjectResult)replay).StatusCode, Is.EqualTo(410));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task CompleteExternalSignIn_ClosedRegistrationOrLockedLinkedAccount_RejectsSignIn(bool locked)
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var users = CreateUserManager();
        if (locked)
        {
            var user = new ApplicationUser { Id = "locked" };
            users.Setup(x => x.FindByLoginAsync("Steam", "76561198000000000")).ReturnsAsync(user);
            users.Setup(x => x.IsLockedOutAsync(user)).ReturnsAsync(true);
        }
        var auth = CreateController(userManager: users);
        var external = StartCompletedFlow(auth, cache);

        var result = await CompleteExternalAsync(auth, external);

        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(403));
    }

    [Test]
    public async Task CompleteExternalSignIn_WrongBrowserCannotAccessFlow()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var auth = CreateController();
        var external = StartCompletedFlow(auth, cache);
        auth.Request.Headers.Remove("Cookie");

        var result = await CompleteExternalAsync(auth, external);

        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(410));
    }

    [Test]
    public async Task CompleteExternalSignIn_EmailVerificationStopsAfterFiveIncorrectCodes()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var auth = CreateController(environmentName: Environments.Development);
        var external = StartCompletedFlow(auth, cache);
        var email = new Mock<IEmailService>();
        string? deliveredCode = null;
        email.Setup(x => x.TrySendVerificationAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Callback<EmailMessage, CancellationToken>((message, _) => deliveredCode = message.Body.Split(' ')[5].TrimEnd('.'))
            .ReturnsAsync(true);
        var registration = new ExternalAccountRegistration(Mock.Of<IServiceScopeFactory>());
        var initial = await CompleteExternalAsync(auth, external);
        var sent = await auth.CompleteExternalSignIn(new("new@example.com", null), external, registration, email.Object);
        Assert.That(initial, Is.TypeOf<AcceptedResult>());
        Assert.That(sent, Is.TypeOf<AcceptedResult>());
        Assert.That(deliveredCode, Has.Length.EqualTo(6));

        var wrongCode = deliveredCode == "000000" ? "000001" : "000000";
        for (var attempt = 1; attempt <= 5; attempt++)
        {
            var result = await auth.CompleteExternalSignIn(new(null, wrongCode), external, registration, email.Object);
            Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(attempt == 5 ? 410 : 400));
        }
    }

    [TestCase("other-owner", 409)]
    [TestCase("another-session", 403)]
    public async Task CompleteExternalSignIn_LinkRejectsOtherOwnerOrBrowserSession(string owner, int expectedStatus)
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var users = CreateUserManager();
        var user = new ApplicationUser { Id = "user-id" };
        users.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(user);
        users.Setup(x => x.FindByLoginAsync("Steam", "76561198000000000"))
            .ReturnsAsync(new ApplicationUser { Id = owner });
        var auth = CreateController(userManager: users);
        Authenticate(auth, owner == "another-session" ? "another-session" : user.Id, DefaultNow);
        var external = StartCompletedFlow(auth, cache, user.Id);

        var result = await CompleteExternalAsync(auth, external);

        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(expectedStatus));
        users.Verify(x => x.AddLoginAsync(It.IsAny<ApplicationUser>(), It.IsAny<UserLoginInfo>()), Times.Never);
    }

    [TestCase(4, true)]
    [TestCase(5, false)]
    public async Task DeleteExternalProfile_RequiresRecentExternalSignIn(int minutesAgo, bool deleted)
    {
        var users = CreateUserManager();
        var user = new ApplicationUser { Id = "user-id" };
        users.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(user);
        users.Setup(x => x.GetLoginsAsync(user)).ReturnsAsync([new UserLoginInfo("Steam", "76561198000000000", "Steam")]);
        users.Setup(x => x.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);
        var auth = CreateController(userManager: users);
        Authenticate(auth, authenticationTime: DefaultNow.AddMinutes(-minutesAgo));
        ((ClaimsIdentity)auth.User.Identity!).AddClaim(new Claim("steamapp_external_provider", "Steam"));

        var result = await auth.DeleteExternalProfile();

        if (deleted)
            Assert.That(result, Is.TypeOf<NoContentResult>());
        else
            Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(403));
        users.Verify(x => x.DeleteAsync(user), deleted ? Times.Once() : Times.Never());
    }

    private static ExternalAuthentication StartCompletedFlow(AuthController auth, IMemoryCache cache, string? userId = null)
    {
        if (auth.ControllerContext.HttpContext == null)
        {
            auth.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        }
        auth.Request.Scheme = "https";
        const string origin = "https://localhost:4300";
        auth.Request.Headers.Origin = origin;
        var external = new ExternalAuthentication(cache, Config(("Authentication:External:FrontendOrigin", origin)),
            CreateTimeProvider(DefaultNow));
        external.Prepare(auth.HttpContext, "Steam", userId);
        auth.Request.Headers.Cookie = auth.Response.Headers.SetCookie[0]!.Split(';')[0];
        var flow = external.GetFlow(auth.HttpContext)!;
        flow.Started = true;
        flow.Login = new UserLoginInfo("Steam", "76561198000000000", "Steam");
        return external;
    }

    private static Task<IActionResult> CompleteExternalAsync(AuthController auth, ExternalAuthentication external) =>
        auth.CompleteExternalSignIn(new(null, null), external,
            new ExternalAccountRegistration(Mock.Of<IServiceScopeFactory>()), Mock.Of<IEmailService>());

    private static AuthController CreateController(
        IReadOnlyList<ClientDefinition>? clients = null,
        Mock<UserManager<ApplicationUser>>? userManager = null,
        Mock<SignInManager<ApplicationUser>>? signInManager = null,
        IConfiguration? configuration = null,
        string environmentName = "Production",
        TimeProvider? timeProvider = null)
    {
        userManager ??= CreateUserManager();
        signInManager ??= CreateSignInManager(userManager);

        return new AuthController(
            new JwtSettings
            {
                Key = JwtKey,
                Issuer = "issuer",
                Audience = "audience",
                DurationMinutes = 60
            },
            clients ?? [],
            userManager.Object,
            signInManager.Object,
            configuration ?? Config(),
            Environment(environmentName),
            timeProvider ?? CreateTimeProvider(DefaultNow),
            NullLogger<AuthController>.Instance);
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManager()
    {
        var userManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            Options.Create(new IdentityOptions()),
            Mock.Of<IPasswordHasher<ApplicationUser>>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            Mock.Of<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Mock.Of<IServiceProvider>(),
            NullLogger<UserManager<ApplicationUser>>.Instance);

        userManager
            .Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(Array.Empty<string>());
        userManager
            .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        return userManager;
    }

    private static Mock<SignInManager<ApplicationUser>> CreateSignInManager(
        Mock<UserManager<ApplicationUser>> userManager)
    {
        return new Mock<SignInManager<ApplicationUser>>(
            userManager.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            Options.Create(new IdentityOptions()),
            NullLogger<SignInManager<ApplicationUser>>.Instance,
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<ApplicationUser>>());
    }

    private static IConfiguration Config(params (string Key, string Value)[] values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values.ToDictionary(x => x.Key, x => (string?)x.Value))
            .Build();
    }

    private static IHostEnvironment Environment(string name)
    {
        var environment = new Mock<IHostEnvironment>();
        environment.SetupGet(x => x.EnvironmentName).Returns(name);
        return environment.Object;
    }

    private static void Authenticate(
        AuthController controller,
        string userId = "user-id",
        DateTimeOffset? authenticationTime = null,
        string? authenticationTimeClaim = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new("scope", SecurityPolicies.UserScope),
            new(UserSessionClaims.Stamp, UserSessionClaims.StampDigest("test-stamp"))
        };

        if (authenticationTime != null)
        {
            claims.Add(new Claim(
                "auth_time",
                authenticationTime.Value.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64));
        }
        else if (authenticationTimeClaim != null)
        {
            claims.Add(new Claim("auth_time", authenticationTimeClaim));
        }

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
            }
        };
    }

    private static TimeProvider CreateTimeProvider(DateTimeOffset utcNow)
    {
        var timeProvider = new Mock<TimeProvider>();
        timeProvider.Setup(x => x.GetUtcNow()).Returns(utcNow);
        return timeProvider.Object;
    }

    private static string Sha256Hex(string value)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }
}
