using Frost.Shared.Models.DTOs;

namespace Frost.Server.Services
{
    public interface IChatDbService
    {
        public Task GetChatDetailsAsync(string chatId);
        public Task<List<ChatPreviewDTO>> GetUserChatsAsync(int userId);
        public Task<bool> IsUserChatAsync(int userId, int chatId);
        public Task<List<int>> GetUserChatsIDAsync(int userId);
    }
    public class ChatDbService
    {
    }
}
