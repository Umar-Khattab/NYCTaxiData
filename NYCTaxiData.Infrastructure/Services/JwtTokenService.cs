// NYCTaxiData.Infrastructure/Services/JwtTokenService.cs
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services;

public class JwtTokenService(IConfiguration _config)
{
    public string GenerateToken(string phoneNumber, string role, string fullName)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Secret"] ?? "default-secret-key-min32chars-longer"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, phoneNumber),
            new Claim(ClaimTypes.Role, role),
            new Claim("FullName", fullName)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "NYCTaxiData",
            audience: _config["Jwt:Audience"] ?? "NYCTaxiData",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}