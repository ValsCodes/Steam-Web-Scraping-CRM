using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SteamApp.Models.ValueObjects.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SteamApp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtSettings _jwtSettings;
    private readonly IReadOnlyList<ClientDefinition> _clients;

    public AuthController(
        JwtSettings jwtSettings,
        IReadOnlyList<ClientDefinition> clients)
    {
        _jwtSettings = jwtSettings;
        _clients = clients;
    }

    [HttpPost("token")]
    public IActionResult Token([FromBody] TokenRequest req)
    {
        var client = _clients
            .SingleOrDefault(c =>
                c.ClientId == req.ClientId &&
                c.ClientSecret == req.ClientSecret);

        if (client == null)
            return Unauthorized("Invalid client credentials.");

        var claims = new[]
        {
            new Claim("client_id", client.ClientId),
            new Claim("scope",     client.AllowedScope)
        };

        var key = new SymmetricSecurityKey(
                         Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(
                         key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow
                                .AddMinutes(_jwtSettings.DurationMinutes),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return Ok(new { token = tokenString });
    }
}

