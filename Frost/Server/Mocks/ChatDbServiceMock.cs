using Frost.Server.Services;
using Frost.Shared.Models.DTOs;
using Frost.Shared.Models;
using System.Linq;
using Frost.Server.Services.Interfaces;

namespace Frost.Server.Mocks
{
    public class ChatDbServiceMock : IChatDbService
    {
        private IUserDbService userDbService;

        public static int latestId = 1;
        private static string message = "Culpa qui officia deserunt mollit anim id est laborum. Sed ut perspiciatis unde omnis iste natus error sit voluptartem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi ropeior architecto beatae vitae dicta sunt.";
        public static List<ChatPreviewDTO> ChatList = new List<ChatPreviewDTO>()
        {
            new ChatPreviewDTO
            {
                Id = latestId++,
                participants = new List<UserDto>()
            {
                new UserDto{
                    Id = 1,
                    Name="Maksym",
                    Email="user1@gmail.com",
                    TelNumber="123456789",
                    ProfileImgUrl = "",
                    Description = "Cześć! Jestem Maksym i poszukuję współlokatora na terenie Warszawy. Mam 24 lata i jestem osobą niepalącą.",
                    City = "Warszawa"
                },

                new UserDto{
                    Id = 2,
                    Name="Ania",
                    Email="user2@gmail.com",
                    TelNumber="123456789",
                    ProfileImgUrl = "",
                    Description = "Cześć! Jestem Ania i poszukuję współlokatora na terenie Warszawy.",
                    City = "Warszawa"
                }
            },
                messages = new List<Message>
                {
                    new Message
                    {
                        content = message,
                        user_Id = 1,
                        userName = "Maksym",
                        sendDate = DateTime.Now,
                    },
                    new Message
                    {
                        content = message,
                        user_Id = 2,
                        userName = "Ania",
                        sendDate = DateTime.Now.AddMinutes(5),
                    },
                    new Message
                    {
                        content = message,
                        user_Id = 1,
                        userName = "Maksym",
                        sendDate = DateTime.Now.AddMinutes(10),
                    }
                }
            },
            new ChatPreviewDTO
            {
                Id = latestId++,
                participants = UserDbServiceMock.GetSampleUsers(),
                messages = new List<Message>
                {
                    new Message
                    {
                        content = message,
                        user_Id = 1,
                        userName = "Maksym",
                        sendDate = DateTime.Now,
                    },
                    new Message
                    {
                        content = message,
                        user_Id = 2,
                        userName = "Ania",
                        sendDate = DateTime.Now.AddMinutes(5),
                    },
                    new Message
                    {
                        content = message,
                        user_Id = 3,
                        userName = "Karol",
                        sendDate = DateTime.Now.AddMinutes(10),
                    }
                }
            },
            new ChatPreviewDTO
            {
                Id = latestId++,
                participants = UserDbServiceMock.GetSampleUsers(),
                messages = new List<Message>
                {
                    new Message
                    {
                        content = message,
                        user_Id = 2,
                        userName = "Ania",
                        sendDate = DateTime.Now,
                    },
                    new Message
                    {
                        content = message,
                        user_Id = 3,
                        userName = "Ania",
                        sendDate = DateTime.Now.AddMinutes(5),
                    },
                    new Message
                    {
                        content = message,
                        user_Id = 2,
                        userName = "Ania",
                        sendDate = DateTime.Now.AddMinutes(10),
                    }
                }
            }

        };
        public ChatDbServiceMock(IUserDbService userDbService) {
            this.userDbService = userDbService;
        }
        public Task GetChatDetailsAsync(string chatId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ChatPreviewDTO>> GetUserChatsAsync(int userId)
        {
            List<ChatPreviewDTO> userChatList = ChatList.Where(c => c.participants.Any(u => u.Id == userId)).ToList();
            await Task.Delay(100);
            return userChatList;
        }

        public async Task<bool> IsUserChatAsync(int userId, int chatId)
        {
            var result = ChatList.Where(c => c.Id == chatId).Any(c => c.participants.Any(u => u.Id == userId));
            await Task.Delay(10);
            return result;
        }

        public async Task<List<int>> GetUserChatsIDAsync(int userId)
        {
            List<int> userChatsID = ChatList.Where(c => c.participants.Any(u => u.Id == userId)).Select(chat => chat.Id).ToList();
            await Task.Delay(10);
            return userChatsID;
        }
    }
}
