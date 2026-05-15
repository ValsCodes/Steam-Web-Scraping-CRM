using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using SteamApp.Domain.ValueObjects.Authentication;
using SteamApp.Infrastructure.Identity;
using SteamApp.WebAPI.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SteamApp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting(SecurityPolicies.AuthRateLimit)]
public class AuthController(
    JwtSettings jwtSettings,
    IReadOnlyList<ClientDefinition> clients,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IConfiguration configuration,
    IHostEnvironment environment,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("token")]
    [AllowAnonymous]
    public IActionResult Token([FromBody] TokenRequest req)
    {
        var client = clients.FirstOrDefault(c =>
            c.ClientId == req.ClientId &&
            ClientSecretMatches(c, req.ClientSecret));

        if (client == null)
            return Unauthorized("Invalid client credentials.");

        var claims = new[]
        {
            new Claim("client_id", client.ClientId),
            new Claim("scope", client.AllowedScope)
        };

        return Ok(CreateTokenResponse(claims));
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (!RegistrationIsEnabled())
        {
            return NotFound();
        }

        var userName = string.IsNullOrWhiteSpace(req.UserName)
            ? req.Email
            : req.UserName.Trim();

        var user = new ApplicationUser
        {
            UserName = userName,
            Email = req.Email
        };

        var result = await userManager.CreateAsync(user, req.Password);

        if (!result.Succeeded)
        {
            return ValidationProblem(CreateModelState(result));
        }

        logger.LogInformation($"User with Email {user.Email} has successfully registerred!");

        return Ok(await CreateUserTokenResponseAsync(user));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await userManager.FindByEmailAsync(req.EmailOrUserName)
            ?? await userManager.FindByNameAsync(req.EmailOrUserName);

        if (user == null)
            return Unauthorized("Invalid user credentials.");

        var result = await signInManager.CheckPasswordSignInAsync(
            user,
            req.Password,
            lockoutOnFailure: true);

        if (result.IsLockedOut)
            return Unauthorized("User account is locked out.");

        if (!result.Succeeded)
            return Unauthorized("Invalid user credentials.");

        logger.LogInformation($"User with Email {user.Email} has successfully logged in");

        return Ok(await CreateUserTokenResponseAsync(user));
    }

    private async Task<AuthResponse> CreateUserTokenResponseAsync(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id),
            new("scope", "user")
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
            claims.Add(new Claim(ClaimTypes.Email, user.Email));

        var roles = await userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return CreateTokenResponse(claims);
    }

    private AuthResponse CreateTokenResponse(IEnumerable<Claim> claims)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(jwtSettings.DurationMinutes);
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.Key));
        var creds = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: creds
        );

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expiresAtUtc
        };
    }

    private bool RegistrationIsEnabled()
    {
        return environment.IsDevelopment() ||
               configuration.GetValue("Authentication:AllowRegistration", false);
    }

    private static bool ClientSecretMatches(
        ClientDefinition client,
        string providedSecret)
    {
        if (string.IsNullOrWhiteSpace(providedSecret))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(client.ClientSecretHash))
        {
            var providedHash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(providedSecret)));

            return FixedTimeEquals(
                client.ClientSecretHash,
                providedHash,
                normalizeHex: true);
        }

        return FixedTimeEquals(client.ClientSecret, providedSecret);
    }

    private static bool FixedTimeEquals(
        string? expected,
        string provided,
        bool normalizeHex = false)
    {
        if (string.IsNullOrWhiteSpace(expected))
        {
            return false;
        }

        if (normalizeHex)
        {
            expected = expected.ToUpperInvariant();
            provided = provided.ToUpperInvariant();
        }

        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var providedBytes = Encoding.UTF8.GetBytes(provided);

        return expectedBytes.Length == providedBytes.Length &&
               CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }

    private static ModelStateDictionary CreateModelState(IdentityResult result)
    {
        var modelState = new ModelStateDictionary();

        foreach (var error in result.Errors)
        {
            modelState.AddModelError(error.Code, error.Description);
        }

        return modelState;
    }
}
