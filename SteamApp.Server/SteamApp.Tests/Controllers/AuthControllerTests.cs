using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using SteamApp.Domain.ValueObjects.Authentication;
using SteamApp.Infrastructure.Identity;
using SteamApp.WebAPI.Controllers;
using SteamApp.WebAPI.Security;

namespace SteamApp.Tests.Controllers;

[TestFixture]
public sealed class AuthControllerTests
{
    private const string JwtKey = "12345678901234567890123456789012";

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
            .Callback<ApplicationUser, string>((user, _) => createdUser = user)
            .ReturnsAsync(IdentityResult.Success);
        userManager
            .Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(Array.Empty<string>());

        var controller = CreateController(
            userManager: userManager,
            environmentName: Environments.Development);

        var result = await controller.Register(new RegisterRequest
        {
            Email = "user@example.com",
            UserName = "  user-name  ",
            Password = "Password1"
        });

        var ok = result as OkObjectResult;
        var response = ok?.Value as AuthResponse;
        var token = new JwtSecurityTokenHandler().ReadJwtToken(response!.Token);

        Assert.Multiple(() =>
        {
            Assert.That(ok, Is.Not.Null);
            Assert.That(createdUser?.UserName, Is.EqualTo("user-name"));
            Assert.That(createdUser?.Email, Is.EqualTo("user@example.com"));
            Assert.That(token.Claims.Any(x => x.Type == "scope" && x.Value == SecurityPolicies.UserScope), Is.True);
        });
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
        });
    }

    [Test]
    public async Task Login_FallsBackToUserNameLookup()
    {
        var user = new ApplicationUser { Id = "user-id", UserName = "tester" };
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

    private static AuthController CreateController(
        IReadOnlyList<ClientDefinition>? clients = null,
        Mock<UserManager<ApplicationUser>>? userManager = null,
        Mock<SignInManager<ApplicationUser>>? signInManager = null,
        IConfiguration? configuration = null,
        string environmentName = "Production")
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
            Environment(environmentName));
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManager()
    {
        return new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            Options.Create(new IdentityOptions()),
            Mock.Of<IPasswordHasher<ApplicationUser>>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            Mock.Of<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Mock.Of<IServiceProvider>(),
            NullLogger<UserManager<ApplicationUser>>.Instance);
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

    private static string Sha256Hex(string value)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }
}
