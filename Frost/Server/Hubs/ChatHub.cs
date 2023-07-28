using Frost.Server.Services;
using Frost.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Frost.Server.Hubs
{
    public interface IChatClient
    {
        Task SendMessageAsync(string message);
        Task ReceiveMessage(Message message);
    }
    [Authorize]
    public class ChatHub : Hub<IChatClient>
    {
        public const string HubUrl = "/chat";
        public IChatDbService chatDbService { get; set; }
        public IMessageDbService messageDbService { get; set; }
        public ChatHub(IChatDbService chatDbService, IMessageDbService messageDbService)
        {
            this.chatDbService = chatDbService;
            this.messageDbService = messageDbService;

        }

        public async Task SendMessage(string chatId, string message)
        {
        
            if (string.IsNullOrEmpty(chatId))
            {
                await base.OnDisconnectedAsync(new Exception("Chat ID is required"));
                return;
            }
            bool validationSuccess = await chatDbService.IsUserChatAsync(int.Parse(Context.UserIdentifier), int.Parse(chatId));
            if (!validationSuccess)
            {
                await base.OnDisconnectedAsync(new Exception("User does not participate in this chat"));
                return;
            }
            var newMessage = await messageDbService.AddNewMessageAsync(int.Parse(Context.UserIdentifier),int.Parse(chatId), message);
            await Clients.Group(chatId).ReceiveMessage(newMessage);
        }

        public override async Task OnConnectedAsync()
        {
            string userID = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userID))
            {
                await base.OnDisconnectedAsync(new Exception("User ID is required"));
                return;
            }
            List<int> IDs = await chatDbService.GetUserChatsIDAsync(int.Parse(userID));
            foreach (int ID in IDs)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, ID.ToString());
            }
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception e)
        {
            await base.OnDisconnectedAsync(e);

        }
    }
}
