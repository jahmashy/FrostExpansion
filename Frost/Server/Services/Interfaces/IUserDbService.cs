using Frost.Server.Models;
using Frost.Shared.Models;
using Frost.Shared.Models.DTOs;

namespace Frost.Server.Services.Interfaces
{
    public interface IUserDbService
    {
        public Task<User> AuthenticateUserAsync(LoginModel loginRequest);
        public Task<(User,bool)> AddUserAsync(RegistrationModel registration);
        public Task<User> GetUserAsync(int userId);
        public Task<User> GetUserAsync(string email);
        public Task DeleteUserAsync(string email);
        public Task UpdateRefreshTokenAsync(string email,string refreshToken);

    }
}
