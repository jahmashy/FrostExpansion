using Frost.Server.EntityModels;
using Frost.Server.Models;
using Frost.Server.Repositories;
using Frost.Server.Services.ImageServices;
using Frost.Server.Services.Interfaces;
using Frost.Shared.Models;
using Frost.Shared.Models.DTOs;
using Frost.Shared.Models.Enums;
using Frost.Shared.Models.Forms;
using Microsoft.EntityFrameworkCore;

namespace Frost.Server.Mocks
{
    public class UserDbServiceMock : IUserRepository
    {

        public static int latestId = 1;
        public static List<MockUserModel> Userlist = new List<MockUserModel>
        {
            new MockUserModel()
            {
                Id=latestId++,
                email="user1@gmail.com",
                password="123",
                name="Maksym",
                telNumber=123456789,
                description = "Cześć! Jestem Maksym i poszukuję współlokatora na terenie Warszawy. Mam 24 lata i jestem osobą niepalącą.",
            },
            new MockUserModel()
            {
                Id=latestId++,
                email="user2@gmail.com",
                password="1234",
                name="Ania",
                telNumber=123456789,
                description = "Cześć! Jestem Ania i poszukuję współlokatora na terenie Warszawy.",
            },
            new MockUserModel()
            {
                Id=latestId++,
                email="user3@gmail.com",
                password="12345",
                name="Karol",
                telNumber=123456789
            },
            new MockUserModel()
            {
                Id=latestId++,
                email="user4@gmail.com",
                password="123456",
                name="Marcin",
                telNumber=123456789
            }
        };
        public static List<(int user1_ID, int user2_ID)> BlockedCommunication = new();
        public IUserImageService _userImageService;
        public UserDbServiceMock(IUserImageService userImageService) {
        _userImageService = userImageService;
        }
        public async Task<(UserDTO, bool)> AddUserAsync(RegistrationModel registration)
        {
            if(Userlist.Where(u => string.Equals(u.email,registration.email)).Count() > 0)
                return (null, true);
            MockUserModel newUser = new MockUserModel(){
            Id= latestId,
            email= registration.email,
            password= registration.password,
            name= registration.name,
            telNumber= (int)registration.telNumber
            };
            Userlist.Add(newUser);
            return (ConvertToDto(newUser), false);
        }

        public async Task<UserDTO> AuthenticateUserAsync(LoginModel loginRequest)
        {
            return await Task.Run(() =>
            {
                var user = Userlist.FirstOrDefault(User => User.email == loginRequest.email);
                if (user == null)
                    return null;
                if (!loginRequest.password.Equals(user.password))
                    return null;
                return ConvertToDto(user);
            });
        }

        public Task DeleteUserAsync(string email)
        {
            throw new NotImplementedException();
        }

        public async Task<UserDTO> GetUserAsync(int userId)
        {
            var user = Userlist.FirstOrDefault(u => u.Id == userId);
            await Task.Delay(10);
            return ConvertToDto(user);
        }
        public async Task<UserDTO> GetUserAsync(string email)
        {
            var user = Userlist.FirstOrDefault(u => u.email == email);
            await Task.Delay(10);
            return ConvertToDto(user);
        }

        public async Task UpdateRefreshTokenAsync(string email, string refreshToken)
        {
            
            var user = Userlist.FirstOrDefault(u => u.email == email);
            user.refreshToken =  refreshToken;
            user.refTokenExpDate = DateTime.UtcNow.AddDays(1);
            await Task.Delay(1000);
        }
        public static List<UserDTO> GetSampleUsers()
        {
            var users = new List<UserDTO>()
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
                },
                new UserDTO{
                    userId = 3,
                    name="Karol",
                    email="user3@gmail.com",
                    telNumber="123456789",
                    profileImgUrl = "",
                    city = "Warszawa"
                }
            };
            return users;
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return Userlist.Where(u=>u.email==email).Any();
        }

        public async Task<bool> UserExistsAsync(int id)
        {
            return Userlist.Where(u => u.Id == id).Any();
        }

        public async Task<UserDTO> UpdateUserLoginDetailsAsync(string email, ChangeLoginDetailsModel loginDetails)
        {
            MockUserModel user = Userlist.Where(u=>u.email == email).First();
            user.email = loginDetails.email ?? user.email;
            user.password = string.IsNullOrEmpty(loginDetails.newPassword)? user.password : loginDetails.newPassword;
            user.telNumber = loginDetails.telNumber ?? user.telNumber;
            return ConvertToDto(user);
        }

        public async Task<bool> UpdateUserDetailsAsync(string email, UserDetailsFormModel userDetails)
        {
            MockUserModel user = Userlist.Where(u => u.email == email).First();
            user.name = userDetails.name;
            user.description = userDetails.description;
            user.nationality = userDetails.nationality;
            user.location = new Location { cityName = userDetails.cityName, cityPlaceId = userDetails.cityPlaceId };
            return true;
        }
        public async Task<bool> BlockUserAsync(int user_ID, int target_user_ID)
        {
            bool alreadyBlocked = BlockedCommunication.Contains((user_ID,target_user_ID));
            if (alreadyBlocked)
                return false;
            BlockedCommunication.Add((user_ID, target_user_ID));
            return true;
        }
        public async Task<bool> IsCommunicationBlockedAsync(int user1_ID, int user2_ID)
        {
            bool isBlocked = BlockedCommunication.Contains((user1_ID, user2_ID)) || BlockedCommunication.Contains((user2_ID, user1_ID));
            return isBlocked;
        }
        public async Task<bool> IsUserBlockedAsync(int user1_ID, int target_user_ID)
        {
            bool isBlocked = BlockedCommunication.Contains((user1_ID, target_user_ID));
            return isBlocked;
        }

        public async Task<bool> UnblockUserAsync(int user_ID, int target_user_ID)
        {
            bool success = BlockedCommunication.Remove((user_ID, target_user_ID));
            return success;
        }

        public async Task<bool> ComparePasswords(int userId, string password)
        {
            return Userlist.FirstOrDefault(u => u.Id == userId).password == password;
        }
        private UserDTO ConvertToDto(MockUserModel user)
        {
            UserDTO userDto = new UserDTO
            {
                userId = user.Id,
                email = user.email,
                name = user.name,
                telNumber = user.telNumber.ToString(),
                description = user.description,
                nationality = user.nationality,
                city = user.location?.cityName,
                cityPlaceId = user.location?.cityPlaceId.ToString(),
                profileImgUrl = _userImageService.GetUserProfileImageUrl(user.Id)
            };
            return userDto;
        }

        public Task<(string refToken, DateTime expDate)> GetUserRefreshTokenAndExpDateAsync(int userId)
        {
            MockUserModel user = Userlist.FirstOrDefault(u => u.Id == userId);
            return Task.FromResult((user.refreshToken, user.refTokenExpDate));
        }

        public Task<string> GetUserRoleAsync(int userId)
        {
            MockUserModel user = Userlist.FirstOrDefault(u => u.Id == userId);
            return Task.FromResult(user.role);
        }

        public Task<NotificationModel> GetUserNotificationsAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ChangeUserNotificationsAsync(int userId, NotificationModel model)
        {
            throw new NotImplementedException();
        }

        public bool DeleteUser(int userId)
        {
            throw new NotImplementedException();
        }
    }
}
