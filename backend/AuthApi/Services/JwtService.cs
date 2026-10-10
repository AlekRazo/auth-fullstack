using AuthApi.Entities;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace AuthApi.Services
{
    public class JwtService(IConfiguration _config)
    {
        public string GenerateToken(User user)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var issuer = jwtSettings["Issuer"]!;
            var audience = jwtSettings["Audience"]!;
            var secretKey = jwtSettings["Key"]!;
            var expirationTime = double.Parse(jwtSettings["ExpirationTime"]!);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, user.IdUser.ToString()),
                new (ClaimTypes.Name, user.Name),
                new (ClaimTypes.Email, user.Email),
                new (ClaimTypes.Role, user.IdRoleNavigation.Name),
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = issuer,
                Audience = audience,
                Expires = DateTime.UtcNow.AddMinutes(expirationTime),
                SigningCredentials = credentials
            };

            var tokenHandler = new JsonWebTokenHandler();
            return tokenHandler.CreateToken(tokenDescriptor);
        }
    }
}
