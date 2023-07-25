using Frost.Server.Models;
using Frost.Server.Services.Interfaces;
using Frost.Shared.Models;
using Frost.Shared.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace Frost.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   
    public class AuthController : ControllerBase
    {
        private IUserDbService userDb;
        private IJWTService JWTService;
        public AuthController(IUserDbService userDb, IJWTService JWTService)
        {
            this.userDb = userDb;
            this.JWTService = JWTService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(LoginModel loginRequest)
        {
            User user = await userDb.AuthenticateUserAsync(loginRequest);
            if (user == null)
                return Unauthorized();

            var token = JWTService.CreateJWT(user);
            var refreshToken = JWTService.GenerateRefreshToken();
            _ = userDb.UpdateRefreshTokenAsync(user.email, refreshToken);
            var Jwt = new JwtSecurityTokenHandler().WriteToken(token);
            var loginResult = new LoginResult
            {
                Id = user.Id,
                name = user.name,
                email = user.email,
                jwt = Jwt,
                jwtRefresh = refreshToken,
                jwtExpDate = token.ValidTo,
            };
            return Ok(loginResult);
                
        }
        [HttpPost("renewToken")]
        public async Task<IActionResult> RenewToken(JwtDTO jwtDTO)
        {
            (ClaimsPrincipal claimsPrincipal, bool validationSuccess) = JWTService.GetPrincipalFromExpiredToken(jwtDTO.token);
            if (!validationSuccess)
                return Unauthorized();
            var userId = claimsPrincipal.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            var user = await userDb.GetUserAsync(int.Parse(userId));
            if (user == null)
                return Unauthorized();
            if(!string.Equals(jwtDTO.refreshToken,user.refreshToken) || DateTime.Compare(user.refTokenExpDate, DateTime.UtcNow) <=0)
                return Unauthorized();
            var token = JWTService.CreateJWT(user);
            var refreshToken = JWTService.GenerateRefreshToken();
            _ = userDb.UpdateRefreshTokenAsync(user.email, refreshToken);
            var Jwt = new JwtSecurityTokenHandler().WriteToken(token);
            var loginResult = new LoginResult
            {
                Id = user.Id,
                name = user.name,
                email = user.email,
                jwt = Jwt,
                jwtRefresh = refreshToken,
                jwtExpDate = token.ValidTo,
            };
            return Ok(loginResult);
        }
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser(RegistrationModel model)
        {
            User user;
            bool userAlreadyExists;
            (user, userAlreadyExists) = await userDb.AddUserAsync(model);
            if(userAlreadyExists) {
                return Conflict();            
            }
            var token = JWTService.CreateJWT(user);
            var refreshToken = JWTService.GenerateRefreshToken();
            _ = userDb.UpdateRefreshTokenAsync(user.email, refreshToken);
            var Jwt = new JwtSecurityTokenHandler().WriteToken(token);
            var loginResult = new LoginResult
            {
                Id = user.Id,
                name = user.name,
                email = user.email,
                jwt = Jwt,
                jwtRefresh = refreshToken,
                jwtExpDate = token.ValidTo,
            };
            return Ok(loginResult);

        }
    }
}
