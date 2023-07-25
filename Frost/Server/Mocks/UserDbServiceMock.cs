using Frost.Server.Models;
using Frost.Server.Services.Interfaces;
using Frost.Shared.Models;
using Frost.Shared.Models.DTOs;

namespace Frost.Server.Mocks
{
    public class UserDbServiceMock : IUserDbService
    {
        public static int latestId = 1;
        public static List<User> Userlist = new List<User>
        {
            new User(latestId++,"user1@gmail.com","123","Maksym","123456789"),
            new User(latestId++,"user2@gmail.com", "1234", "Ania", "123456789"),
            new User(latestId++,"user3@gmail.com", "12345", "Karol", "123456789"),
            new User(latestId++,"user4@gmail.com", "123456", "Marcin", "123456789")
        };
        public async Task<(User, bool)> AddUserAsync(RegistrationModel registration)
        {
            if(Userlist.Where(u => string.Equals(u.email,registration.email)).Count() > 0)
                return (null, true);
            User newUser = new User(latestId++,registration.email, registration.password, registration.name, registration.telNumber);
            Userlist.Add(newUser);
            return (newUser, false);
        }

        public async Task<User> AuthenticateUserAsync(LoginModel loginRequest)
        {
            return await Task.Run(() =>
            {
                var user = Userlist.FirstOrDefault(User => User.email == loginRequest.email);
                if (user == null)
                    return null;
                if (!loginRequest.password.Equals(user.password))
                    return null;
                return user;
            });
        }

        public Task DeleteUserAsync(string email)
        {
            throw new NotImplementedException();
        }

        public async Task<User> GetUserAsync(int userId)
        {
            var user = Userlist.FirstOrDefault(u => u.Id == userId);
            await Task.Delay(10);
            return user;
        }
        public async Task<User> GetUserAsync(string email)
        {
            var user = Userlist.FirstOrDefault(u => u.email == email);
            await Task.Delay(10);
            return user;
        }

        public async Task UpdateRefreshTokenAsync(string email, string refreshToken)
        {
            
            var user = Userlist.FirstOrDefault(u => u.email == email);
            user.refreshToken =  refreshToken;
            user.refTokenExpDate = DateTime.UtcNow.AddDays(1);
            await Task.Delay(1000);
        }
        public static List<UserDto> GetSampleUsers()
        {
            var users = new List<UserDto>()
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
                },
                new UserDto{
                    Id = 3,
                    Name="Karol",
                    Email="user3@gmail.com",
                    TelNumber="123456789",
                    ProfileImgUrl = "",
                    City = "Warszawa"
                }
            };
            return users;
        }
    }
}
