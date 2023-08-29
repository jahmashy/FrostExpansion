using Frost.Server.Models;
using Frost.Shared.Models.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Frost.Server.Services.Interfaces
{
    public interface IJWTService
    {
        public JwtSecurityToken CreateJWT(UserDTO user, string role);
        public Claim[] GetClaims(UserDTO user, string role);
        public SigningCredentials GetSigningCredentials();
        public string GenerateRefreshToken();
        public (ClaimsPrincipal?, bool) GetPrincipalFromExpiredToken(string token);
        
    }
}
