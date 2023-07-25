using Frost.Shared.Models;

namespace Frost.Server.Services
{
    public interface IMessageDbService
    {
        public Task<Message> AddNewMessageAsync(int userId, int chatId, string message);
    }
    public class MessageDbService
    {
    }
}
