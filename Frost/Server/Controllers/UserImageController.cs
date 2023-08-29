using Frost.Server.Services.ImageServices;
using Frost.Shared.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Frost.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserImageController : ControllerBase
    {
        private IUserImageService _userImageService;
        public UserImageController(IUserImageService userImageService, IConfiguration configuration)
        {
            _userImageService = userImageService;
        }
        [HttpGet("{userId}")]
        public IActionResult GetUserImage(int userId) {
            FileStatusCode status;
            Byte[] data;
            (status,data) = _userImageService.GetUserProfileImage(userId);
            if(status == FileStatusCode.FileDoesNotExists)
                return NotFound();
            return File(data, "image/webp");
        }
        [Authorize]
        [HttpDelete("{userId}")]
        public IActionResult DeleteUserImage(int userId)
        {
            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(bearer_token);
            string id = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            if (int.Parse(id) != userId)
                return Unauthorized();
            _userImageService.DeleteUserProfileImage(userId);
            return Ok();
        }
    }
}
