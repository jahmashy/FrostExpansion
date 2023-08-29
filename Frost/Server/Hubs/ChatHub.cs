using Frost.Server.Repositories;
using Frost.Server.Services.Interfaces;
using Frost.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Frost.Server.Hubs
{
    public interface IChatClient
    {
        Task SendMessageAsync(string message);
        Task ReceiveMessage(MessageModel message);
        Task ConnectToNewChat(int newChatId);
        Task NotifyAboutJoin();
        Task NotifyAboutLeave();
    }
    [Authorize]
    public class ChatHub : Hub<IChatClient>
    {
        public const string HubUrl = "/chat";
        public IChatRepository _chatRepository { get; set; }
        public IUserRepository _userRepository { get; set; }
        public ChatHub(IChatRepository chatRepository, IUserRepository userRepository)
        {
            _chatRepository = chatRepository;
            _userRepository = userRepository;

        }

        public async Task SendMessage(string chatId, string message)
        {
            int maxMsgLength = 4096;
            if (string.IsNullOrEmpty(chatId))
            {
                await base.OnDisconnectedAsync(new Exception("Chat ID is required"));
                return;
            }
            if(message.Length > maxMsgLength)
            {
                await base.OnDisconnectedAsync(new Exception("Message is too long"));
                return;
            }
            bool validationSuccess = await _chatRepository.IsUserInChatAsync(int.Parse(Context.UserIdentifier), int.Parse(chatId));
            if (!validationSuccess)
            {
                await base.OnDisconnectedAsync(new Exception("User does not participate in this chat"));
                return;
            }
            MessageModel newMessage;
            bool success;
            (newMessage,success) = await _chatRepository.AddNewMessageAsync(int.Parse(Context.UserIdentifier),int.Parse(chatId), message);
            if (success)
                await Clients.Group($"chat_{chatId}").ReceiveMessage(newMessage);
        }

        public override async Task OnConnectedAsync()
        {
            string userID = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userID))
            {
                await base.OnDisconnectedAsync(new Exception("User ID is required"));
                return;
            }
            List<int> IDs = await _chatRepository.GetUserChatsIDAsync(int.Parse(userID));
            foreach (int ID in IDs)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{ID}");
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userID}");

            await base.OnConnectedAsync();
        }
        public async Task AddClientToGroupAsync(string chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatId}");
        }
        public async Task RemoveClientFromGroupAsync(int chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat_{chatId}");
        }
        public async Task NotifyUserAboutNewChat(int targetUserId, int newChatID)
        {
            string userID = Context.UserIdentifier;
            if (!await _userRepository.IsCommunicationBlockedAsync(int.Parse(userID), targetUserId)) {
                await Clients.Group($"user_{targetUserId}").ConnectToNewChat(newChatID);
            }
            
        }
        public async Task NotifyUsersAboutJoining(string chatID)
        {
            await Clients.Group($"chat_{chatID}").NotifyAboutJoin();
        }

        public override async Task OnDisconnectedAsync(Exception e)
        {
            await base.OnDisconnectedAsync(e);

        }
        public async Task NotifyUsersAboutLeaving(int chatID)
        {
            await Clients.Group($"chat_{chatID}").NotifyAboutLeave();
        }
    }
}
