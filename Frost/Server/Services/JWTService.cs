using Frost.Server.Models;
using Frost.Server.Services.Interfaces;
using Frost.Shared.Models.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Frost.Server.Services
{
    public class JWTService : IJWTService
    {
        private readonly IConfiguration configuration;
        private readonly IConfigurationSection jwtSettings;
        public JWTService(IConfiguration configuration) 
        {
            this.configuration = configuration;
            this.jwtSettings = configuration.GetSection("JwtSettings");
        }
        public JwtSecurityToken CreateJWT(User user)
        {
            var userclaims = GetClaims(user);
            var creds = GetSigningCredentials();

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: "localhost:44350",
                audience: "localhost:44350",
                claims: userclaims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );
            return token;
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public Claim[] GetClaims(User user)
        {
            Claim[] userclaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Email,user.email),
                new Claim(ClaimTypes.Name,user.name),
                new Claim(ClaimTypes.Role,user.role)
            };
            return userclaims;
        }

        public SigningCredentials GetSigningCredentials()
        {
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["IssuerSigningKey"]));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            return creds;
        }
        public (ClaimsPrincipal?,bool) GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["IssuerSigningKey"])),
                ValidAudience = jwtSettings["ValidAudience"],
                ValidIssuer = jwtSettings["ValidIssuer"]
                
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
                var jwtSecurityToken = securityToken as JwtSecurityToken;
                return (principal, true);
            }
            catch
            {
                return (null, false);
            }
        }
 
    }
}
