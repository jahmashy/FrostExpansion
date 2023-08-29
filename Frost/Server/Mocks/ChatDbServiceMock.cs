using Frost.Shared.Models.DTOs;
using Frost.Shared.Models;
using System.Linq;
using Frost.Server.Services.Interfaces;
using Frost.Server.Models;
using Frost.Shared.Models.Enums;
using Frost.Server.Services.ImageServices;
using Frost.Server.Repositories;

namespace Frost.Server.Mocks
{
    public class ChatDbServiceMock : IChatRepository
    {
        private IUserRepository userDbService;
        private IUserImageService userImageService;

        public static int latestId = 1;
        public static int maxChatUsers = 20;
        private static string message = "Culpa qui officia deserunt mollit anim id est laborum. Sed ut perspiciatis unde omnis iste natus error sit voluptartem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi ropeior architecto beatae vitae dicta sunt.";
        public static List<ChatPreviewDTO> ChatList = new List<ChatPreviewDTO>()
        {
            new ChatPreviewDTO
            {
                Id = latestId++,
                isGroupChat = false,
                participants = new List<UserDTO>()
            {
                new UserDTO{
                    userId = 1,
                    name="Maksym",
                    email="user1@gmail.com",
                    telNumber="123456789",
                    profileImgUrl = "",
                    description = "Cześć! Jestem Maksym i poszukuję współlokatora na terenie Warszawy. Mam 24 lata i jestem osobą niepalącą.",
                    city = "Warszawa"
                },

                new UserDTO{
                    userId = 2,
                    name="Ania",
                    email="user2@gmail.com",
                    telNumber="123456789",
                    profileImgUrl = "",
                    description = "Cześć! Jestem Ania i poszukuję współlokatora na terenie Warszawy.",
                    city = "Warszawa"
                }
            },
                messages = new List<MessageModel>
                {
                    new MessageModel
                    {
                        content = message,
                        user_Id = 1,
                        userName = "Maksym",
                        sendDate = DateTime.UtcNow,
                    },
                    new MessageModel
                    {
                        content = message,
                        user_Id = 2,
                        userName = "Ania",
                        sendDate = DateTime.UtcNow,
                    },
                    new MessageModel
                    {
                        content = message,
                        user_Id = 1,
                        userName = "Maksym",
                        sendDate = DateTime.UtcNow,
                    }
                }
                
            },
            new ChatPreviewDTO
            {
                Id = latestId++,
                isGroupChat = true,
                participants = UserDbServiceMock.GetSampleUsers(),
                messages = new List<MessageModel>
                {
                    new MessageModel
                    {
                        content = message,
                        user_Id = 1,
                        userName = "Maksym",
                        sendDate = DateTime.UtcNow,
                    },
                    new MessageModel
                    {
                        content = message,
                        user_Id = 2,
                        userName = "Ania",
                        sendDate = DateTime.UtcNow,
                    },
                    new MessageModel
                    {
                        content = message,
                        user_Id = 3,
                        userName = "Karol",
                        sendDate = DateTime.UtcNow,
                    }
                }
            },
            new ChatPreviewDTO
            {
                Id = latestId++,
                isGroupChat = true,
                participants = UserDbServiceMock.GetSampleUsers(),
                messages = new List<MessageModel>
                {
                    new MessageModel
                    {
                        content = message,
                        user_Id = 2,
                        userName = "Ania",
                        sendDate = DateTime.UtcNow,
                    },
                    new MessageModel
                    {
                        content = message,
                        user_Id = 3,
                        userName = "Ania",
                        sendDate = DateTime.UtcNow,
                    },
                    new MessageModel
                    {
                        content = message,
                        user_Id = 2,
                        userName = "Ania",
                        sendDate = DateTime.UtcNow,
                    }
                }
            }

        };
        public static List<ChatInvitationModel> ChatInvitationsList { get; set; } = new List<ChatInvitationModel>();
        public ChatDbServiceMock(IUserRepository userDbService, IUserImageService imageService) {
            this.userDbService = userDbService;
            this.userImageService = imageService;
        }
        public Task GetChatDetailsAsync(string chatId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ChatPreviewDTO>> GetUserChatsAsync(int userId)
        {
            List<ChatPreviewDTO> userChatList = ChatList.Where(c => c.participants.Any(u => u.userId == userId)).OrderByDescending(c => c.messages.LastOrDefault()?.sendDate).ToList();
            await Task.Delay(100);
            return userChatList;
        }

        public async Task<bool> IsUserInChatAsync(int userId, int chatId)
        {
            var result = ChatList.Where(c => c.Id == chatId).Any(c => c.participants.Any(u => u.userId == userId));
            await Task.Delay(10);
            return result;
        }

        public async Task<List<int>> GetUserChatsIDAsync(int userId)
        {
            List<int> userChatsID = ChatList.Where(c => c.participants.Any(u => u.userId == userId)).Select(chat => chat.Id).ToList();
            await Task.Delay(10);
            return userChatsID;
        }

        public async Task<ChatStatusCode> AddUserToChatAsync(int chatId, int userId)
        {
            var chat = ChatList.Where(c=> c.Id == chatId).FirstOrDefault();
            if (chat == null)
                return ChatStatusCode.ChatDoesNotExists;
            if (chat.participants.Count() >= maxChatUsers)
                return ChatStatusCode.MaxParticipantsReached;
            if (!chat.isGroupChat && chat.participants.Count() > 2)
                return ChatStatusCode.ChatIsNotGroupChat;
            var user = await userDbService.GetUserAsync(userId);
            if (user == null) 
                return ChatStatusCode.UserDoesNotExists;
            if (await IsUserInChatAsync(userId, chatId))
                return ChatStatusCode.UserIsAlreadyInChat;
            chat.participants.Add(user);
            return ChatStatusCode.Success;
        }

        public async Task<(ChatPreviewDTO?, bool)> CreatePrivateChatAsync(int user1_Id, int user2_Id)
        {
            var user1 = await userDbService.GetUserAsync(user1_Id);
            var user2 = await userDbService.GetUserAsync (user2_Id);
            if (user1 == null || user2 == null)
                return (null, false);
            var chat = new ChatPreviewDTO() {
                Id = latestId++,
                isGroupChat = false,
                participants = new List<UserDTO>() {user1,user2},
                messages = new List<MessageModel>()
            };
            ChatList.Add(chat);
            return (chat,true);
        }
        public async Task<(ChatPreviewDTO?, ChatStatusCode)> CreateGroupChatAsync(int userId)
        {
            var user = await userDbService.GetUserAsync(userId);
            if (user is null)
                return (null, ChatStatusCode.UserDoesNotExists);
            var chat = new ChatPreviewDTO()
            {
                Id = latestId++,
                isGroupChat = true,
                participants = new List<UserDTO>() {
                    user
                },
                messages = new List<MessageModel>()
            };
            ChatList.Add(chat);
            return (chat, ChatStatusCode.Success);
        }

        public async Task<ChatStatusCode> RemoveUserFromChatAsync(int chatId, int userId)
        {
            var user = await userDbService.GetUserAsync(userId);
            if (user == null)
                return ChatStatusCode.UserDoesNotExists;
            var chat = ChatList.Where(c=> c.Id == chatId).FirstOrDefault();
            if (chat == null)
                return ChatStatusCode.ChatDoesNotExists;
            var userInChat = chat.participants.Where(u => u.userId == userId).FirstOrDefault();
            if (userInChat == null)
                return ChatStatusCode.UserIsNotInChat;
            chat.participants.Remove(userInChat);
            return ChatStatusCode.Success;
        }

        public Task<bool> DeleteChatAsync(int chatId, int userId)
        {
            throw new NotImplementedException();
        }

        public async Task<(MessageModel? returnedMessage, bool success)> AddNewMessageAsync(int userId, int chatId, string message)
        {
            Task.Delay(1);
            var user = await userDbService.GetUserAsync(userId);
            if (user == null)
                return (null,false);
            var newMessage = new MessageModel()
            {
                content = message,
                user_Id = user.userId,
                userName = user.name,
                sendDate = DateTime.UtcNow,
                chat_Id = chatId,
            };
            var chat = ChatList.Where(c => c.Id == chatId).FirstOrDefault();
            if (chat == null)
                return (null, false);
            if(!chat.isGroupChat && chat.participants.Count() == 2 && await userDbService.IsCommunicationBlockedAsync(userId,chat.participants.Where(p=>p.userId!=userId).Select(p=>p.userId).First()))
                return (null,false);
            chat.messages.Add(newMessage);
            return (newMessage,true);
        }
        public async Task<(ChatStatusCode status,string id)> CreateChatInvitation(int chatId) {
            var chat = ChatList.Where(c => c.Id == chatId).FirstOrDefault();
            if (chat == null)
                return (ChatStatusCode.ChatDoesNotExists,null);
            ChatInvitationModel chatInv = new ChatInvitationModel
            {
                id = Guid.NewGuid().ToString(),
                chatID = chatId,
                expirationDate = DateTime.UtcNow.AddDays(3),
            };
            ChatInvitationsList.Add(chatInv);
            return (ChatStatusCode.Success,chatInv.id);
        }
        public async Task<(ChatStatusCode,int chatID)> ConsumeChatInvitation(string invitationID, int userID)
        {
            ChatInvitationModel chatInv = ChatInvitationsList.FirstOrDefault(i => i.id == invitationID);
            if(chatInv is null)
                return (ChatStatusCode.InvitationIsNotValid,0);
            if (DateTime.Compare(chatInv.expirationDate, DateTime.UtcNow) < 0)
            {
                ChatInvitationsList.Remove(chatInv);
                return (ChatStatusCode.InvitationHasExpired, 0);
            }
                
            var status = await AddUserToChatAsync(chatInv.chatID, userID);
            if (status == ChatStatusCode.Success)
                ChatInvitationsList.Remove(chatInv);
            return (status,chatInv.chatID);
        }


    }
    
}
