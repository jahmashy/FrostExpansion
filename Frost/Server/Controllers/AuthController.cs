using Frost.Server.Models;
using Frost.Server.Repositories;
using Frost.Server.Services.Interfaces;
using Frost.Shared.Models;
using Frost.Shared.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace Frost.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   
    public class AuthController : ControllerBase
    {
        private IUserRepository userDb;
        private IJWTService JWTService;
        public AuthController(IUserRepository userDb, IJWTService JWTService)
        {
            this.userDb = userDb;
            this.JWTService = JWTService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(LoginModel loginRequest)
        {
            UserDTO user = await userDb.AuthenticateUserAsync(loginRequest);
            if (user == null)
                return Unauthorized();

            var userRole = await userDb.GetUserRoleAsync(user.userId);

            var token = JWTService.CreateJWT(user, userRole);
            var refreshToken = JWTService.GenerateRefreshToken();
            await userDb.UpdateRefreshTokenAsync(user.email, refreshToken);
            var Jwt = new JwtSecurityTokenHandler().WriteToken(token);
            var loginResult = new LoginResult
            {
                Id = user.userId,
                name = user.name,
                email = user.email,
                telNumber = int.Parse(user.telNumber),
                jwt = Jwt,
                jwtRefresh = refreshToken,
                jwtExpDate = token.ValidTo,
                Role = userRole,
            };
            return Ok(loginResult);
                
        }
        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RenewToken(JwtDTO jwtDTO)
        {
            (ClaimsPrincipal claimsPrincipal, bool validationSuccess) = JWTService.GetPrincipalFromExpiredToken(jwtDTO.token);
            if (!validationSuccess)
                return Unauthorized();
            string userId = claimsPrincipal.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            UserDTO user = await userDb.GetUserAsync(int.Parse(userId));
            if (user == null)
                return Unauthorized();

            string userRefreshToken;
            DateTime refTokenExpDate;
            (userRefreshToken, refTokenExpDate) = await userDb.GetUserRefreshTokenAndExpDateAsync(int.Parse(userId));

            if (!string.Equals(jwtDTO.refreshToken, userRefreshToken) || DateTime.Compare(refTokenExpDate, DateTime.UtcNow) <= 0)
                return Unauthorized();

            string userRole = await userDb.GetUserRoleAsync(int.Parse(userId));

            JwtSecurityToken token = JWTService.CreateJWT(user, userRole);
            string refreshToken = JWTService.GenerateRefreshToken();
            await userDb.UpdateRefreshTokenAsync(user.email, refreshToken);
            var Jwt = new JwtSecurityTokenHandler().WriteToken(token);
            var loginResult = new LoginResult
            {
                Id = user.userId,
                name = user.name,
                email = user.email,
                telNumber = int.Parse(user.telNumber),
                jwt = Jwt,
                jwtRefresh = refreshToken,
                jwtExpDate = token.ValidTo,
                Role = userRole,
            };
            return Ok(loginResult);
        }
        [HttpPost]
        public async Task<IActionResult> RegisterUser(RegistrationModel model)
        {
            UserDTO user;
            bool userDoesntExists;
            (user, userDoesntExists) = await userDb.AddUserAsync(model);
            if(!userDoesntExists) {
                return Conflict();            
            }
            var token = JWTService.CreateJWT(user,"User");
            var refreshToken = JWTService.GenerateRefreshToken();
            await userDb.UpdateRefreshTokenAsync(user.email, refreshToken);
            var Jwt = new JwtSecurityTokenHandler().WriteToken(token);
            var loginResult = new LoginResult
            {
                Id = user.userId,
                name = user.name,
                email = user.email,
                jwt = Jwt,
                jwtRefresh = refreshToken,
                jwtExpDate = token.ValidTo,
                Role = "User"
            };
            return Ok(loginResult);
        }
        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> ChangeUserLoginDetails(ChangeLoginDetailsModel model)
        {
            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(bearer_token);
            string email = jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            string id = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var targetUser = await userDb.GetUserAsync(model.email);
            bool userEmailAlreadyExists = false;
            if (targetUser is not null)
                userEmailAlreadyExists = targetUser.userId != int.Parse(id);
            if(userEmailAlreadyExists)
                return Conflict();
            if(!await userDb.ComparePasswords(int.Parse(id),model.password))
                return Unauthorized();
            UserDTO user;
            if (!string.IsNullOrEmpty(model.newPassword))
            {
                user = await userDb.AuthenticateUserAsync(new LoginModel { password = model.password, email = model.email});
                if(user is null)
                    return Unauthorized();
            }
            user = await userDb.UpdateUserLoginDetailsAsync(email, model);

            string userRole = await userDb.GetUserRoleAsync(int.Parse(id));

            var token = JWTService.CreateJWT(user, userRole);
            var refreshToken = JWTService.GenerateRefreshToken();
            await userDb.UpdateRefreshTokenAsync(user.email, refreshToken);
            var Jwt = new JwtSecurityTokenHandler().WriteToken(token);
            var loginResult = new LoginResult
            {
                Id = user.userId,
                name = user.name,
                email = user.email,
                telNumber = int.Parse(user.telNumber),
                jwt = Jwt,
                jwtRefresh = refreshToken,
                jwtExpDate = token.ValidTo,
                Role = userRole,

            };
            return Ok(loginResult);
        }
    }
}
