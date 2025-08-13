using Microsoft.IdentityModel.Tokens;
using ProjectGreenLens.Services.Interfaces;
using ProjectGreenLens.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProjectGreenLens.Services.Implementations
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly byte[] _keyBytes;

        public JwtService(JwtSettings jwtSettings)
        {
            _jwtSettings = jwtSettings;
            _keyBytes = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
        }

        public Task<string> GenerateTokenAsync(string userId, string role)
        {

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Role, role)
                };

            var credentials = new SigningCredentials(new SymmetricSecurityKey(_keyBytes), SecurityAlgorithms.HmacSha256);

            var securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                Expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpiryMinutes),
                SigningCredentials = credentials;
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(securityTokenDescriptor);
            return Task.FromResult(tokenHandler.WriteToken(token));
        }

        public Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null!;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,

                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(_keyBytes),

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                return Task.FromResult<ClaimsPrincipal?>(principal);

            }
            catch
            {
                return Task.FromResult<ClaimsPrincipal?>(null);
            }
        }
    }
}
