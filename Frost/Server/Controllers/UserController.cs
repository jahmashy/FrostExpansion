using BrunoZell.ModelBinding;
using Frost.Client.Shared.Components.UserAccountComponents;
using Frost.Server.Models;
using Frost.Server.Repositories;
using Frost.Server.Services.ImageServices;
using Frost.Server.Services.Interfaces;
using Frost.Shared.Models;
using Frost.Shared.Models.DTOs;
using Frost.Shared.Models.Enums;
using Frost.Shared.Models.Forms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;

namespace Frost.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IUserRepository userRepository;
        private IUserImageService userImageService;
        private IAuthService authService;
        public UserController(IUserRepository userRepository, IUserImageService userImageService, IAuthService authService) {
            this.userRepository = userRepository;
            this.userImageService = userImageService;
            this.authService = authService;
        }
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser(int userId)
        {
            var user = await userRepository.GetUserAsync(userId);
            if (user == null)
                return NotFound();

            return Ok(user);
        }
        [HttpPut("{userId}")]
        [Authorize]
        public async Task<IActionResult> UpdateUserDetails([ModelBinder(BinderType = typeof(JsonModelBinder))] UserDetailsFormModel userDetails, [FromForm] IFormFile? profilePicture, int userId)

        {
            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(bearer_token);
            string usermail = jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value;

            if (!authService.AuthorizeUserIdentityFromId(userId, bearer_token))
                return Unauthorized();

            FileStatusCode fileStatus;
            bool handleFiles = profilePicture is not null;
            int propertyId;
            long maxFileSize = 1024 * 1024 * 5;
            string[] allowExtensions = { ".jpg", ".png", ".jpeg", ".webp" };
            if (handleFiles)
            {
                fileStatus = userImageService.ValidateImage(profilePicture, maxFileSize, allowExtensions);
                if (fileStatus != FileStatusCode.Success)
                    return BadRequest(fileStatus.ToString());
            }
            await userRepository.UpdateUserDetailsAsync(usermail, userDetails);
            if (handleFiles)
                await userImageService.SaveImageAsync(profilePicture, userId.ToString());

            return Ok();
        }
        [HttpPut("BlockUser")]
        [Authorize]
        public async Task<IActionResult> BlockUser(BlockUserModel model)
        {
            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            if (!authService.AuthorizeUserIdentityFromId(model.userId, bearer_token))
                return Unauthorized();

            var success = await userRepository.BlockUserAsync(model.userId, model.targetUserId);
            if (!success)
                return Conflict("User is already blocked");
            return Ok();
        }
        [HttpPut("UnblockUser")]
        [Authorize]
        public async Task<IActionResult> UnblockUser(BlockUserModel model)
        {
            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            if (!authService.AuthorizeUserIdentityFromId(model.userId, bearer_token))
                return Unauthorized();

            bool success = await userRepository.UnblockUserAsync(model.userId, model.targetUserId);
            if (!success)
                return NotFound("User is not blocked");
            return Ok();
        }
        [HttpGet("UserBlockStatus")]
        public async Task<IActionResult> IsUserBlocked([FromQuery] int userId, [FromQuery] int targetUserId)
        {
            var status = await userRepository.IsUserBlockedAsync(userId, targetUserId);
            return Ok(status);
        }
        [HttpGet("CommunicationStatus")]
        public async Task<IActionResult> IsCommunicationBlocked([FromQuery] int userId, [FromQuery] int targetUserId)
        {
            var status = await userRepository.IsCommunicationBlockedAsync(userId, targetUserId);
            return Ok(status);
        }
        [HttpGet("{userId}/notifications")]
        public async Task<IActionResult> GetUserNotificationsStatus(int userId)
        {
            NotificationModel model = await userRepository.GetUserNotificationsAsync(userId);
            if(model == null)
                return NotFound();
            return Ok(model);
        }
        [Authorize]
        [HttpPut("{userId}/notifications")]
        public async Task<IActionResult> ChangeUserNotificationsStatus(int userId,NotificationModel model)
        {
            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(bearer_token);
            string id = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            int userIdFromToken = int.Parse(id);
            if (userIdFromToken != userId)
                return Unauthorized();
            bool success = await userRepository.ChangeUserNotificationsAsync(userId, model);
            if(success)
                return Ok(model);
            return StatusCode(500);
        }
        [Authorize]
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(bearer_token);
            string id = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            string role = jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value;
            int userIdFromToken = int.Parse(id);
            if (userIdFromToken != userId && role != "Admin")
                return Unauthorized();
            bool success = userRepository.DeleteUser(userId);
            if(!success) return NotFound();
            return Ok();
        }
    }
}
