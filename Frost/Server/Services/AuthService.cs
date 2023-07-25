using Frost.Server.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Frost.Server.Services
{
    public class AuthService : IAuthService
    {
        public bool AuthorizeUserIdentityFromEmail(string email, string token)
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            string userEmail = jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            if(string.Equals(userEmail,email))
                return true;
            return false;
        }
        public bool AuthorizeUserIdentityFromId(int id, string token)
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            string userId = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            if (string.Equals(userId, id.ToString()))
                return true;
            return false;
        }
    }
}
