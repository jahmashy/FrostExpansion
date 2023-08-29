using Frost.Server.EntityModels;
using Frost.Server.Services.ImageServices;
using Frost.Server.Services.MailServices;
using Frost.Shared.Models;
using Frost.Shared.Models.DTOs;
using Frost.Shared.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Frost.Server.Repositories
{
    public interface IChatRepository
    {
        public Task GetChatDetailsAsync(string chatId);
        public Task<List<ChatPreviewDTO>> GetUserChatsAsync(int userId);
        public Task<bool> IsUserInChatAsync(int userId, int chatId);
        public Task<List<int>> GetUserChatsIDAsync(int userId);
        public Task<ChatStatusCode> AddUserToChatAsync(int chatId, int userId);
        public Task<ChatStatusCode> RemoveUserFromChatAsync(int chatId, int userId);
        public Task<(ChatPreviewDTO?, bool)> CreatePrivateChatAsync(int user1_Id, int user2_Id);
        public Task<(ChatPreviewDTO?, ChatStatusCode)> CreateGroupChatAsync(int userId);
        public Task<bool> DeleteChatAsync(int chatId, int userId);
        public Task<(MessageModel? returnedMessage, bool success)> AddNewMessageAsync(int userId, int chatId, string message);
        public Task<(ChatStatusCode status, string id)> CreateChatInvitation(int chatId);
        public Task<(ChatStatusCode, int chatID)> ConsumeChatInvitation(string invitationID, int userID);
    }
    public class ChatRepository : IChatRepository
    {
        private FrostDbContext _dbContext;
        private IUserRepository _userDbService;
        private IUserImageService _userImageService;
        private IMailService _mailService;

        private static int maxChatUsers = 20;
        public ChatRepository(FrostDbContext dbContext, IUserRepository userDbService, IUserImageService userImageService, IMailService mailService)
        {
            _dbContext = dbContext;
            _userDbService = userDbService;
            _userImageService = userImageService;
            _mailService = mailService;
        }
        public async Task<(MessageModel? returnedMessage, bool success)> AddNewMessageAsync(int userId, int chatId, string message)
        {
            User user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return (null, false);
            Chat chat = _dbContext.Chats.FirstOrDefault(c => c.Id == chatId);
            if (chat == null)
                return (null, false);

            if (!chat.GroupChat)
            {
                List<User> participants = _dbContext.Users.Where(u => u.Id != userId).Join(_dbContext.ChatRooms.Where(cr => cr.Chat == chat), user => user.Id, chatroom => chatroom.UserId, (user, chatroom) => user).ToList();
                if (await _userDbService.IsCommunicationBlockedAsync(userId, participants.First().Id))
                    return (null, false);
            }
            Message msg = new Message
            {
                MessageContent = message,
                User = user,
                MessageDate = DateTime.UtcNow,
                Chat = chat
            };
            chat.Messages.Add(msg);
            _dbContext.SaveChanges();

            MessageModel msgDto = new MessageModel
            {
                content = msg.MessageContent,
                sendDate = msg.MessageDate,
                user_Id = userId,
                userName = user.Name,
                chat_Id = chat.Id,
            };
            return (msgDto, true);
        }

        public async Task<ChatStatusCode> AddUserToChatAsync(int chatId, int userId)
        {
            User user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return ChatStatusCode.UserDoesNotExists;
            Chat chat = _dbContext.Chats.FirstOrDefault(c => c.Id == chatId);
            if (chat == null)
                return ChatStatusCode.ChatDoesNotExists;
            if (await IsUserInChatAsync(userId, chatId))
                return ChatStatusCode.UserIsAlreadyInChat;

            List<User> participants = _dbContext.Users.Join(_dbContext.ChatRooms.Where(cr => cr.Chat == chat), user => user.Id, chatroom => chatroom.UserId, (user, chatroom) => user).ToList();
            if (participants.Count > maxChatUsers)
                return ChatStatusCode.MaxParticipantsReached;

            if (!chat.GroupChat && participants.Count() >= 2)
                return ChatStatusCode.ChatIsNotGroupChat;
            await _dbContext.ChatRooms.AddAsync(new ChatRoom
            {
                User = user,
                Chat = chat,
            });
            await _dbContext.SaveChangesAsync();
            return ChatStatusCode.Success;
        }

        public async Task<(ChatStatusCode, int chatID)> ConsumeChatInvitation(string invitationID, int userID)
        {
            ChatInvitation invitation = await _dbContext.ChatInvitations.FirstOrDefaultAsync(ci => ci.Id == invitationID);
            if (invitation is null)
                return (ChatStatusCode.InvitationIsNotValid, 0);
            if (DateTime.Compare(invitation.ExpirationDate, DateTime.UtcNow) < 0)
            {
                _dbContext.ChatInvitations.Remove(invitation);
                return (ChatStatusCode.InvitationHasExpired, 0);
            }
            var status = await AddUserToChatAsync(invitation.ChatId, userID);
            if (status == ChatStatusCode.Success)
                _dbContext.ChatInvitations.Remove(invitation);
            await _dbContext.SaveChangesAsync();
            return (status, invitation.ChatId);
        }

        public async Task<(ChatStatusCode status, string id)> CreateChatInvitation(int chatId)
        {
            Chat chat = await _dbContext.Chats.FirstOrDefaultAsync(c => c.Id == chatId);
            if (chat == null)
                return (ChatStatusCode.ChatDoesNotExists, null);
            ChatInvitation invitation = new ChatInvitation
            {
                Id = Guid.NewGuid().ToString(),
                Chat = chat,
                ExpirationDate = DateTime.UtcNow.AddDays(3),
            };
            _dbContext.ChatInvitations.Add(invitation);
            await _dbContext.SaveChangesAsync();
            return (ChatStatusCode.Success, invitation.Id);
        }

        public async Task<(ChatPreviewDTO?, ChatStatusCode)> CreateGroupChatAsync(int userId)
        {
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return (null, ChatStatusCode.UserDoesNotExists);
            Chat chat = new Chat { GroupChat = true };
            ChatRoom chatroom = new ChatRoom { User = user, Chat = chat };
            _dbContext.ChatRooms.Add(chatroom);
            await _dbContext.SaveChangesAsync();
            ChatPreviewDTO chatDto = ConvertToDto(chat, new List<User> { user });
            return (chatDto, ChatStatusCode.Success);

        }

        public async Task<(ChatPreviewDTO?, bool)> CreatePrivateChatAsync(int user1_Id, int user2_Id)
        {
            User user1 = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == user1_Id);
            User user2 = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == user2_Id);
            if (user1 == null || user2 == null) return (null, false);

            Chat chat = new Chat()
            {
                GroupChat = false
            };
            ChatRoom chatroom = new ChatRoom()
            {
                User = user1,
                Chat = chat
            };
            ChatRoom chatroom2 = new ChatRoom()
            {
                User = user2,
                Chat = chat
            };

            _dbContext.ChatRooms.Add(chatroom);
            _dbContext.ChatRooms.Add(chatroom2);
            int result = await _dbContext.SaveChangesAsync();
            if (result > 0 && user2.NotifyAboutNewMessages == true)
                _mailService.NotifyUserAboutNewMessageAsync(user2.Email, user1.Name);
            return (ConvertToDto(chat, new List<User> { user1, user2 }), true);
        }

        public Task<bool> DeleteChatAsync(int chatId, int userId)
        {
            throw new NotImplementedException();
        }

        public Task GetChatDetailsAsync(string chatId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ChatPreviewDTO>> GetUserChatsAsync(int userId)
        {
            List<ChatPreviewDTO> userChats = new List<ChatPreviewDTO>();

            List<Chat> chats = _dbContext.Chats
                .Include(c => c.Messages)
                .Join(_dbContext.ChatRooms.Where(cr => cr.UserId == userId), chat => chat.Id, chatroom => chatroom.ChatId, (chat, chatroom) => chat)
                .OrderByDescending(c => c.Messages.FirstOrDefault().MessageDate)
                .ToList();
            foreach (Chat chat in chats)
            {
                List<User> participants = _dbContext.Users.Join(_dbContext.ChatRooms.Where(cr => cr.Chat == chat), user => user.Id, chatroom => chatroom.UserId, (user, chatroom) => user).ToList();
                userChats.Add(ConvertToDto(chat, participants));
            }

            return userChats;
        }

        public async Task<List<int>> GetUserChatsIDAsync(int userId)
        {
            List<int> chatIds = _dbContext.Chats.Join(_dbContext.ChatRooms.Where(cr => cr.UserId == userId), chat => chat.Id, chatroom => chatroom.ChatId, (chat, chatroom) => chat).Select(c => c.Id).ToList();
            return chatIds;
        }

        public async Task<bool> IsUserInChatAsync(int userId, int chatId)
        {
            return await _dbContext.ChatRooms.Where(cr => cr.UserId == userId && cr.ChatId == chatId).AnyAsync();
        }

        public async Task<ChatStatusCode> RemoveUserFromChatAsync(int chatId, int userId)
        {
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return ChatStatusCode.UserDoesNotExists;
            Chat chat = await _dbContext.Chats.FirstOrDefaultAsync(c => c.Id == chatId);
            if (chat == null)
                return ChatStatusCode.ChatDoesNotExists;
            bool isUserInChat = await IsUserInChatAsync(userId, chatId);
            if (!isUserInChat)
                return ChatStatusCode.UserIsNotInChat;
            ChatRoom cr = await _dbContext.ChatRooms.Where(cr => cr.UserId == userId && cr.ChatId == chatId).FirstAsync();
            _dbContext.ChatRooms.Remove(cr);
            _dbContext.SaveChanges();
            return ChatStatusCode.Success;
        }
        private ChatPreviewDTO ConvertToDto(Chat chat, List<User> participants)
        {
            List<MessageModel> messages = new List<MessageModel>();
            foreach (var message in chat.Messages)
            {
                messages.Add(new MessageModel
                {
                    content = message.MessageContent,
                    sendDate = message.MessageDate,
                    user_Id = message.UserId,
                    userName = _dbContext.Users.First(u => u.Id == message.UserId).Name,
                    chat_Id = message.ChatId
                });
            }
            List<UserDTO> participantsDto = new List<UserDTO>();
            foreach (var participant in participants)
            {
                UserDTO userDto = new UserDTO();
                userDto.userId = participant.Id;
                userDto.email = participant.Email;
                userDto.name = participant.Name;
                userDto.telNumber = participant.TelNumber.ToString();
                userDto.description = participant.Description;
                userDto.nationality = participant.Nationality is null ? Nationality.None : (Nationality)Enum.Parse(typeof(Nationality), participant.Nationality, true);
                userDto.city = participant.City?.Name ?? "";
                userDto.cityPlaceId = participant.CityId?.ToString() ?? "";
                userDto.profileImgUrl = _userImageService.GetUserProfileImageUrl(participant.Id);
                participantsDto.Add(userDto);
            }
            return new ChatPreviewDTO
            {
                Id = chat.Id,
                isGroupChat = chat.GroupChat,
                messages = messages,
                participants = participantsDto
            };

        }
    }
}
