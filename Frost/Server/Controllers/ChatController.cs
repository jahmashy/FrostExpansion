using Frost.Server.Services.Interfaces;
using Frost.Server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;

namespace Frost.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private IAuthService authService;
        private IChatDbService chatDbService;

        public ChatController(IAuthService authService, IChatDbService chatDbService)
        {
            this.authService = authService;
            this.chatDbService = chatDbService;
        }

        [Authorize]
        [HttpGet("userChats/{userId}")]
        public async Task<IActionResult> GetUserChats(int userId)
        {
            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            if (authService.AuthorizeUserIdentityFromId(userId, bearer_token))
            {
                var chats = await chatDbService.GetUserChatsAsync(userId);
                return Ok(chats);
            }
            else
            {
                return Unauthorized();
            }

        }
    }
}
