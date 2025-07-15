using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SteamApp.Models.ValueObjects.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SteamApp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    JwtSettings jwtSettings,
    IReadOnlyList<ClientDefinition> clients) : ControllerBase
{

    [HttpPost("token")]
    public IActionResult Token([FromBody] TokenRequest req)
    {
        var client = clients
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
                         Encoding.UTF8.GetBytes(jwtSettings.Key));
        var creds = new SigningCredentials(
                         key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(jwtSettings.DurationMinutes),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return Ok(new { token = tokenString });
    }

    [HttpGet("token-expiration")]
    [Authorize]
    public IActionResult TokenExpiration(string tokenString)
    {
        var handler = new JwtSecurityTokenHandler();

        try
        {
            if (handler.ReadToken(tokenString) is not JwtSecurityToken jwt)
            {
                return Ok(null);
            }

            DateTime expiration = jwt.ValidTo.ToLocalTime();

            return Ok(expiration);
        }
        catch
        {
            return BadRequest("Bad Token");
        }
    }
}

