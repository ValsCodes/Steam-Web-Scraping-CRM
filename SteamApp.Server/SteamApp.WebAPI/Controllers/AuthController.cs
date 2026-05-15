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

        var email = req.Email.Trim();
        var userName = string.IsNullOrWhiteSpace(req.UserName)
            ? email
            : req.UserName.Trim();

        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            PhoneNumber = NormalizeOptional(req.Phone),
            FirstName = NormalizeOptional(req.FirstName),
            LastName = NormalizeOptional(req.LastName)
        };

        var result = await userManager.CreateAsync(user, req.Password);

        if (!result.Succeeded)
        {
            return ValidationProblem(CreateModelState(result));
        }

        logger.LogInformation("User with email {Email} has successfully registered.", user.Email);

        return Ok(await CreateUserTokenResponseAsync(user));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var emailOrUserName = req.EmailOrUserName.Trim();
        var user = await userManager.FindByEmailAsync(emailOrUserName)
            ?? await userManager.FindByNameAsync(emailOrUserName);

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

        logger.LogInformation("User with email {Email} has successfully logged in.", user.Email);

        return Ok(await CreateUserTokenResponseAsync(user));
    }

    [HttpGet("profile")]
    [Authorize(Policy = SecurityPolicies.ApiUser)]
    public async Task<IActionResult> GetProfile()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return Unauthorized("User profile is unavailable.");
        }

        return Ok(CreateUserProfileResponse(user));
    }

    [HttpPut("profile")]
    [Authorize(Policy = SecurityPolicies.ApiUser)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequest req)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return Unauthorized("User profile is unavailable.");
        }

        var email = req.Email.Trim();

        user.FirstName = NormalizeOptional(req.FirstName);
        user.LastName = NormalizeOptional(req.LastName);
        user.Email = email;
        user.UserName = string.IsNullOrWhiteSpace(req.UserName)
            ? email
            : req.UserName.Trim();
        user.PhoneNumber = NormalizeOptional(req.Phone);

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return ValidationProblem(CreateModelState(result));
        }

        logger.LogInformation("User {UserId} updated their profile.", user.Id);

        return Ok(CreateUserProfileResponse(user));
    }

    [HttpPut("profile/password")]
    [Authorize(Policy = SecurityPolicies.ApiUser)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return Unauthorized("User profile is unavailable.");
        }

        var result = await userManager.ChangePasswordAsync(
            user,
            req.CurrentPassword,
            req.NewPassword);

        if (!result.Succeeded)
        {
            return ValidationProblem(CreateModelState(result));
        }

        logger.LogInformation("User {UserId} changed their password.", user.Id);

        return NoContent();
    }

    [HttpDelete("profile")]
    [Authorize(Policy = SecurityPolicies.ApiUser)]
    public async Task<IActionResult> DeleteProfile([FromBody] DeleteUserRequest req)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return Unauthorized("User profile is unavailable.");
        }

        if (!await userManager.CheckPasswordAsync(user, req.Password))
        {
            return Unauthorized("Invalid user credentials.");
        }

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return ValidationProblem(CreateModelState(result));
        }

        logger.LogInformation("User {UserId} deleted their account.", user.Id);

        return NoContent();
    }

    private async Task<AuthResponse> CreateUserTokenResponseAsync(ApplicationUser user)
    {
        var displayName = CreateDisplayName(user)
            ?? user.UserName
            ?? user.Email
            ?? user.Id;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, displayName),
            new("scope", SecurityPolicies.UserScope)
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
            claims.Add(new Claim(ClaimTypes.Email, user.Email));

        if (!string.IsNullOrWhiteSpace(user.UserName))
            claims.Add(new Claim("preferred_username", user.UserName));

        if (!string.IsNullOrWhiteSpace(user.FirstName))
            claims.Add(new Claim("given_name", user.FirstName));

        if (!string.IsNullOrWhiteSpace(user.LastName))
            claims.Add(new Claim("family_name", user.LastName));

        if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
            claims.Add(new Claim("phone_number", user.PhoneNumber));

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

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return await userManager.FindByIdAsync(userId);
    }

    private static UserProfileResponse CreateUserProfileResponse(ApplicationUser user)
    {
        return new UserProfileResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            UserName = user.UserName,
            Email = user.Email,
            Phone = user.PhoneNumber,
            DisplayName = CreateDisplayName(user)
                ?? user.UserName
                ?? user.Email
                ?? user.Id
        };
    }

    private static string? CreateDisplayName(ApplicationUser user)
    {
        var fullName = string.Join(
            ' ',
            new[] { user.FirstName, user.LastName }
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value!.Trim()));

        return string.IsNullOrWhiteSpace(fullName) ? null : fullName;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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
