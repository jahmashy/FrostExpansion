using Frost.Server.Models;
using Frost.Shared.Models.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Frost.Server.Services.Interfaces
{
    public interface IJWTService
    {
        public JwtSecurityToken CreateJWT(User user);
        public Claim[] GetClaims(User user);
        public SigningCredentials GetSigningCredentials();
        public string GenerateRefreshToken();
        public (ClaimsPrincipal?, bool) GetPrincipalFromExpiredToken(string token);
        
    }
}
