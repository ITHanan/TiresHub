using ApplicationLayer.Interfaces;
using DomainLayer.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InfrastructureLayer.Helpers
{
    public class JWTGenerator : IJwtGenerator
    {
        private readonly IConfiguration _configuration;

        public JWTGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");

            var keyString = jwtSettings["Key"];
            if (string.IsNullOrWhiteSpace(keyString))
            {
                throw new InvalidOperationException(
                    "JWT Key is missing. Check JwtSettings:Key in appsettings.json");
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(keyString));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.UserEmail),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var expiresMinutes = double.TryParse(
                jwtSettings["ExpireMinutes"], out var minutes)
                ? minutes
                : 60;

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
