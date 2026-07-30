using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using YaUsers.Application.DTO;
using YaUsers.Application.Services;
using YaUsers.Domain.Exceptions;

namespace YaUsers.Infrastructure
{
    public class JWTService : IJWTService
    {
        private readonly byte[] _jwtKey;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly double _jwtLifetime;

        public JWTService(IConfiguration configuration)
        {
            var key = configuration["Jwt:Key"] ?? throw new ConfigurationException("Jwt Key Missing");
            _jwtKey = Encoding.UTF8.GetBytes(key);
            _jwtIssuer = configuration["Jwt:Issuer"] ?? throw new ConfigurationException("Jwt Issuer Missing");
            _jwtAudience = configuration["Jwt:Audience"] ?? throw new ConfigurationException("Jwt Audience Missing");
            var lifetime = configuration["Jwt:Lifetime"] ?? throw new ConfigurationException("Jwt Lifetime Missing");
            _jwtLifetime = Convert.ToDouble(lifetime);
        }

        public string GenerateToken(UserInfoDto user)
        {
            var claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
                ["login"] = user.Login,
                ["role"] = user.Role.ToString()
            };

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _jwtIssuer,
                Audience = _jwtAudience,
                Claims = claims,
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(_jwtLifetime),
                IssuedAt = DateTime.UtcNow,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_jwtKey), SecurityAlgorithms.HmacSha256)
            };

            return new JsonWebTokenHandler().CreateToken(descriptor);
        }
    }
}
