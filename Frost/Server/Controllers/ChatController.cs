using Frost.Server.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Frost.Shared.Models;
using System.Web.Http.Results;
using Frost.Shared.Models.Enums;
using System.Net;
using Frost.Shared.Models.DTOs;
using System.Reflection;
using Frost.Server.Repositories;

namespace Frost.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private IAuthService authService;
        private IChatRepository chatDbService;
        private IUserRepository userDbService;

        public ChatController(IAuthService authService, IChatRepository chatDbService, IUserRepository userDbService)
        {
            this.authService = authService;
            this.chatDbService = chatDbService;
            this.userDbService = userDbService;
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
        [Authorize]
        [HttpPatch("AddUser")]
        public async Task<IActionResult> AddUserToChat(ChatUserModel model)
        {
            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(bearer_token);
            string id = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if (await chatDbService.IsUserInChatAsync(int.Parse(id), model.chatId))
                return Forbid();

            if (await userDbService.IsCommunicationBlockedAsync(int.Parse(id), model.userId))
                return Forbid(ChatStatusCode.CommunicationIsBlocked.ToString());

            ChatStatusCode status = await chatDbService.AddUserToChatAsync(model.chatId, model.userId);

            switch (status)
            {
                case ChatStatusCode.Success:
                    {
                        return Ok(status.ToString());
                    }
                case ChatStatusCode.ChatDoesNotExists:
                    {
                        return NotFound(status.ToString());
                    }
                case ChatStatusCode.UserDoesNotExists:
                    {
                        return NotFound(status.ToString());
                    }
                case ChatStatusCode.MaxParticipantsReached:
                    {
                        return StatusCode(406);
                    }
                case ChatStatusCode.UserIsAlreadyInChat:
                    {
                        return Conflict(status.ToString());
                    }
                default:
                    return StatusCode(500);
            }

        }
        [Authorize]
        [HttpDelete("{chatId}/{userId}")]
        public async Task<IActionResult> RemoveUserFromChat(int chatId, int userId)
        {
            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(bearer_token);
            string id = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            int userIdFromToken = int.Parse(id);

            if (userId != userIdFromToken)
                return Unauthorized();

            if (!await chatDbService.IsUserInChatAsync(userId,chatId))
                return Forbid();


            ChatStatusCode status = await chatDbService.RemoveUserFromChatAsync(chatId, userId);

            switch (status)
            {
                case ChatStatusCode.Success:
                    {
                        return Ok(status.ToString());
                    }
                case ChatStatusCode.ChatDoesNotExists:
                    {
                        return NotFound(status.ToString());
                    }
                case ChatStatusCode.UserDoesNotExists:
                    {
                        return NotFound(status.ToString());
                    }
                case ChatStatusCode.UserIsNotInChat:
                    {
                        return Conflict(status.ToString());
                    }
                default:
                    return StatusCode(500);
            }
        }
        [Authorize]
        [HttpPost("privatechat")]
        public async Task<IActionResult> CreatePrivateChat(CreateChatModel model)
        {
            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(bearer_token);
            string id = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            int userId = int.Parse(id);

            if (model.user_id != userId)
                return Unauthorized();
            if (await userDbService.IsCommunicationBlockedAsync(model.user_id, model.targetUser_id))
                return Unauthorized();

            ChatPreviewDTO chat;
            bool createChatSuccess;

            (chat, createChatSuccess) = await chatDbService.CreatePrivateChatAsync(model.user_id, model.targetUser_id);
            if (createChatSuccess)
            {
                bool addMessageSuccess;
                MessageModel message;
                (message, addMessageSuccess) = await chatDbService.AddNewMessageAsync(model.user_id, chat.Id, model.firstMessage);
                if (addMessageSuccess)
                    return Ok(chat);

            }
            return StatusCode(500);

        }
        [Authorize]
        [HttpPost("groupchat")]
        public async Task<IActionResult> CreateGroupChat([FromBody]int userID)
        {
            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(bearer_token);
            string id = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            int userIdFromToken = int.Parse(id);

            if (userIdFromToken != userID)
                return Unauthorized();


            ChatPreviewDTO chat;
            ChatStatusCode status;

            (chat, status) = await chatDbService.CreateGroupChatAsync(userID);
            switch (status)
            {
                case ChatStatusCode.Success:
                    {
                        return Ok(chat);
                    }
                case ChatStatusCode.UserDoesNotExists:
                    {
                        return NotFound(status.ToString());
                    }
                default:
                    return StatusCode(500,status);
            }
        }
        [Authorize]
        [HttpPost("invitation")]
        public async Task<IActionResult> CreateChatInvitation([FromBody] int chatID)
        {

            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(bearer_token);
            int userID = int.Parse(jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

            if (!await chatDbService.IsUserInChatAsync(userID, chatID))
                return Unauthorized(ChatStatusCode.UserIsNotInChat.ToString());
            string invitationID;
            ChatStatusCode status;
            (status,invitationID) = await chatDbService.CreateChatInvitation(chatID);
            switch (status)
            {
                case ChatStatusCode.Success:
                    {
                        return Ok(invitationID);
                    }
                case ChatStatusCode.ChatDoesNotExists:
                    {
                        return NotFound(status.ToString());
                    }
                default:
                    return StatusCode(500);
            }
        }
        [Authorize]
        [HttpPatch("invitation/{invitationID}")]
        public async Task<IActionResult> ConsumeChatInvitation(string invitationID)
        {
            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(bearer_token);
            int userId = int.Parse(jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

            ChatStatusCode status;
            int chatID;
            (status,chatID)= await chatDbService.ConsumeChatInvitation(invitationID, userId);
            switch (status)
            {
                case ChatStatusCode.Success:
                    {
                        return Ok(chatID);
                    }
                case ChatStatusCode.ChatDoesNotExists:
                    {
                        return NotFound(status.ToString());
                    }
                case ChatStatusCode.UserDoesNotExists:
                    {
                        return NotFound(status.ToString());
                    }
                case ChatStatusCode.UserIsAlreadyInChat:
                    {
                        return Conflict(status.ToString());
                    }
                case ChatStatusCode.InvitationHasExpired:
                    {
                        return StatusCode(410,status.ToString());
                    }
                case ChatStatusCode.InvitationIsNotValid:
                    {
                        return NotFound(status.ToString());
                    }
                default:
                    return StatusCode(500);
            }
        }
    }
}
