using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EasyData.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace EasyData.Api.Services;

public class TokenService(IConfiguration c)
{
    public string Crear(Usuario u)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(c["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, u.Nombre),
            new Claim(ClaimTypes.NameIdentifier, u.Id.ToString()),
            new Claim(ClaimTypes.Role, u.Rol)
        };
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}