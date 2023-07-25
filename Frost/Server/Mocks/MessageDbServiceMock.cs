using Frost.Server.Services;
using Frost.Server.Services.Interfaces;
using Frost.Shared.Models;
using System.Linq;

namespace Frost.Server.Mocks
{
    public class MessageDbServiceMock : IMessageDbService
    {
        private IUserDbService userDbService;
        public MessageDbServiceMock(IUserDbService userDbService) {
            this.userDbService = userDbService;
        }
        public async Task<Message> AddNewMessageAsync(int userId, int chatId, string message)
        {
            Task.Delay(1);
            var user = await userDbService.GetUserAsync(userId);
            var newMessage = new Message()
            {
                content = message,
                user_Id = user.Id,
                userName = user.name,
                sendDate = DateTime.Now,
                chatroom_Id = chatId,
            };
            ChatDbServiceMock.ChatList
                .Where(c => c.Id == chatId)
                .Select(c => c.messages)
                .First()
                .Add(newMessage);
            return newMessage;
        }
    }
}
