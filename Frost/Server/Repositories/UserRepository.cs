using Frost.Shared.Models.Forms;
using Frost.Shared.Models;
using Frost.Server.EntityModels;
using Microsoft.EntityFrameworkCore;
using Frost.Shared.Models.DTOs;
using Frost.Shared.Models.Enums;
using Frost.Server.Services.Interfaces;
using System.Security.Cryptography;
using Frost.Server.Services.MailServices;
using Frost.Server.Services.ImageServices;
using Frost.Server.Services.AuthServices;

namespace Frost.Server.Repositories
{
    public interface IUserRepository
    {
        public Task<UserDTO> AuthenticateUserAsync(LoginModel loginRequest);
        public Task<(UserDTO, bool)> AddUserAsync(RegistrationModel registration);
        public Task<UserDTO?> GetUserAsync(int userId);
        public Task<UserDTO?> GetUserAsync(string email);
        public bool DeleteUser(int userId);
        public Task UpdateRefreshTokenAsync(string email, string refreshToken);
        public Task<bool> UserExistsAsync(string email);
        public Task<bool> UserExistsAsync(int id);
        public Task<UserDTO?> UpdateUserLoginDetailsAsync(string email, ChangeLoginDetailsModel loginDetails);
        public Task<bool> UpdateUserDetailsAsync(string email, UserDetailsFormModel userDetails);
        public Task<bool> BlockUserAsync(int user_ID, int target_user_ID);
        public Task<bool> UnblockUserAsync(int user_ID, int target_user_ID);
        public Task<bool> IsCommunicationBlockedAsync(int user1_ID, int user2_ID);
        public Task<bool> IsUserBlockedAsync(int user1_ID, int targetUser_Id);
        public Task<bool> ComparePasswords(int userId, string password);
        public Task<(string refToken, DateTime expDate)> GetUserRefreshTokenAndExpDateAsync(int userId);
        public Task<string> GetUserRoleAsync(int userId);
        public Task<NotificationModel> GetUserNotificationsAsync(int userId);
        public Task<bool> ChangeUserNotificationsAsync(int userId, NotificationModel model);
    }
    public class UserRepository : IUserRepository
    {
        private FrostDbContext _dbContext;
        private IUserImageService _userImageService;
        private IJWTService _jwtService;
        private IMailService _mailService;
        public UserRepository(FrostDbContext dbContext, IUserImageService userImageService, IJWTService jWTService, IMailService mailService)
        {
            _dbContext = dbContext;
            _userImageService = userImageService;
            _jwtService = jWTService;
            _mailService = mailService;
        }

        public async Task<(UserDTO, bool)> AddUserAsync(RegistrationModel registration)
        {
            if (_dbContext.Users.Where(u => string.Equals(u.Email, registration.email) || u.TelNumber == registration.telNumber).Count() > 0)
                return (null, false);
            string hashedPassword;
            string salt;
            (hashedPassword, salt) = SecurityService.GetHashedPasswordAndSalt(registration.password);
            User user = new User()
            {
                Email = registration.email,
                Password = hashedPassword,
                PasswordSalt = salt,
                Name = registration.name,
                TelNumber = (int)registration.telNumber,
                RefreshToken = _jwtService.GenerateRefreshToken(),
                RefreshTokenExpirationDate = DateTime.UtcNow.AddDays(7),
                Role = _dbContext.Roles.First(r => r.RoleName == "User")
            };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            return (ConvertToDto(user), true);

        }

        public async Task<UserDTO> AuthenticateUserAsync(LoginModel loginRequest)
        {
            User user = await _dbContext.Users.Include(u => u.City).FirstOrDefaultAsync(User => User.Email == loginRequest.email);
            if (user is null)
                return null;
            string enteredHashedPass = SecurityService.GetHashedPasswordWithSalt(loginRequest.password, user.PasswordSalt);
            if (!string.Equals(user.Password, enteredHashedPass))
                return null;
            return ConvertToDto(user);
        }

        public async Task<bool> BlockUserAsync(int user_ID, int target_user_ID)
        {
            bool alreadyBlocked = await _dbContext.BlockedCommunications.Where(b => b.User1.Id == user_ID && b.User2.Id == target_user_ID).AnyAsync();
            if (alreadyBlocked)
                return false;
            await _dbContext.BlockedCommunications.AddAsync(new BlockedCommunication() { User1Id = user_ID, User2Id = target_user_ID });
            await _dbContext.SaveChangesAsync();
            return true;

        }

        public async Task<bool> ComparePasswords(int userId, string password)
        {
            User user = await _dbContext.Users.FirstOrDefaultAsync(User => User.Id == userId);
            if (user is null)
                return false;
            string enteredHashedPass = SecurityService.GetHashedPasswordWithSalt(password, user.PasswordSalt);
            if (string.Equals(user.Password, enteredHashedPass))
                return true;
            return false;
        }

        public bool DeleteUser(int userId)
        {
            User user = _dbContext.Users.FirstOrDefault(User => User.Id == userId);
            if (user is null)
                return false;
            _dbContext.ChatRooms.RemoveRange(_dbContext.ChatRooms.Where(x => x.UserId == user.Id));
            _dbContext.Messages.RemoveRange(_dbContext.Messages.Where(x => x.UserId == user.Id));
            _dbContext.BlockedCommunications.RemoveRange(_dbContext.BlockedCommunications.Where(x => x.User1Id == user.Id || x.User2Id == user.Id));
            _dbContext.ViewCounts.RemoveRange(_dbContext.ViewCounts.Where(x => x.UserId == user.Id));
            _dbContext.PropertyRoommates.RemoveRange(_dbContext.PropertyRoommates.Where(x => x.UserId == user.Id));
            _dbContext.FollowedOffers.RemoveRange(_dbContext.FollowedOffers.Where(x => x.UserId == user.Id));
            List<Property> properties = _dbContext.Properties.Where(x => x.UserId == user.Id).ToList();
            foreach (var property in properties)
            {
                _dbContext.ViewCounts.RemoveRange(_dbContext.ViewCounts.Where(x => x.PropertyId == property.Id));
                _dbContext.PropertyRoommates.RemoveRange(_dbContext.PropertyRoommates.Where(x => x.PropertyId == property.Id));
                _dbContext.PropertyRoommates.RemoveRange(_dbContext.PropertyRoommates.Where(x => x.PropertyId == property.Id));
                _dbContext.Properties.Remove(property);
                _dbContext.SaveChanges();
            }
            _dbContext.Users.Remove(user);
            _dbContext.SaveChanges();
            return true;

        }

        public async Task<UserDTO?> GetUserAsync(int userId)
        {
            User user = await _dbContext.Users.Include(u => u.City).FirstOrDefaultAsync(user => user.Id == userId);
            if (user is null)
                return null;
            return ConvertToDto(user);
        }

        public async Task<UserDTO?> GetUserAsync(string email)
        {
            User user = await _dbContext.Users.Include(u => u.City).FirstOrDefaultAsync(user => user.Email == email);
            if (user is null)
                return null;
            return ConvertToDto(user);
        }

        public async Task<(string refToken, DateTime expDate)> GetUserRefreshTokenAndExpDateAsync(int userId)
        {
            User user = await _dbContext.Users.FirstAsync(u => u.Id == userId);
            return (user.RefreshToken, user.RefreshTokenExpirationDate);
        }
        public async Task<string> GetUserRoleAsync(int userId)
        {
            return _dbContext.Users.Include(u => u.Role).FirstOrDefault(u => u.Id == userId).Role.RoleName;
        }

        public async Task<bool> IsCommunicationBlockedAsync(int user1_ID, int user2_ID)
        {
            return await _dbContext.BlockedCommunications.Where(bc => bc.User1Id == user1_ID && bc.User2Id == user2_ID || bc.User2Id == user1_ID && bc.User1Id == user2_ID).AnyAsync();
        }

        public async Task<bool> IsUserBlockedAsync(int user1_ID, int targetUser_Id)
        {
            bool alreadyBlocked = await _dbContext.BlockedCommunications.AnyAsync(b => b.User1.Id == user1_ID && b.User2.Id == targetUser_Id);
            return alreadyBlocked;

        }

        public async Task<bool> UnblockUserAsync(int user_ID, int target_user_ID)
        {
            if (!await IsUserBlockedAsync(user_ID, target_user_ID))
                return false;
            _dbContext.BlockedCommunications.Remove(await _dbContext.BlockedCommunications.Where(b => b.User1Id == user_ID && b.User2Id == target_user_ID).FirstAsync());
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task UpdateRefreshTokenAsync(string email, string refreshToken)
        {
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return;
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpirationDate = DateTime.UtcNow.AddDays(7);
            await _dbContext.SaveChangesAsync();
            return;
        }

        public async Task<bool> UpdateUserDetailsAsync(string email, UserDetailsFormModel userDetails)
        {
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user is null) return false;
            if (!string.IsNullOrEmpty(userDetails.cityName))
            {
                if (!await _dbContext.Cities.AnyAsync(c => c.GoogleId == userDetails.cityPlaceId))
                {
                    await _dbContext.Cities.AddAsync(new City { Name = userDetails.cityName, GoogleId = userDetails.cityPlaceId });
                    _dbContext.SaveChanges();
                }

                user.City = await _dbContext.Cities.FirstAsync(c => c.GoogleId == userDetails.cityPlaceId);
            }
            else
            {
                user.City = null;
            }
            user.Name = userDetails.name;
            user.Description = userDetails.description;
            user.Nationality = userDetails.nationality.ToString();
            _dbContext.SaveChanges();
            return true;

        }

        public async Task<UserDTO?> UpdateUserLoginDetailsAsync(string email, ChangeLoginDetailsModel loginDetails)
        {
            User user = await _dbContext.Users.Include(u => u.City).FirstOrDefaultAsync(u => u.Email == email);
            if (user is null)
                return null;

            user.Email = loginDetails.email ?? user.Email;
            if (!string.IsNullOrEmpty(loginDetails.newPassword))
            {
                string hashedPassword;
                string salt;
                (hashedPassword, salt) = SecurityService.GetHashedPasswordAndSalt(loginDetails.newPassword);
                user.Password = hashedPassword;
                user.PasswordSalt = salt;
            }

            user.TelNumber = loginDetails.telNumber ?? user.TelNumber;

            int result = await _dbContext.SaveChangesAsync();
            if (result > 0 && user.NotifyAboutChangedLoginDetails == true)
                _mailService.NotifyUserAboutChangedLoginDetailsAsync(user.Email);
            return ConvertToDto(user);

        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _dbContext.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> UserExistsAsync(int id)
        {
            return await _dbContext.Users.AnyAsync(u => u.Id == id);
        }
        public async Task<bool> ChangeUserNotificationsAsync(int userId, NotificationModel model)
        {
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;
            user.NotifyAboutChangedLoginDetails = model.NotifyAboutChangedLoginDetails;
            user.NotifyAboutExpiringOffers = model.NotifyAboutExpiringOffers;
            user.NotifyAboutNewMessages = model.NotifyAboutNewMessages;
            user.NotifyAboutNewOffers = model.NotifyAboutNewOffers;
            await _dbContext.SaveChangesAsync();
            return true;
        }
        public async Task<NotificationModel> GetUserNotificationsAsync(int userId)
        {
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;
            NotificationModel model = new NotificationModel();
            model.NotifyAboutChangedLoginDetails = user.NotifyAboutChangedLoginDetails;
            model.NotifyAboutExpiringOffers = user.NotifyAboutExpiringOffers;
            model.NotifyAboutNewMessages = user.NotifyAboutNewMessages;
            model.NotifyAboutNewOffers = user.NotifyAboutNewOffers;
            return model;
        }

        public UserDTO ConvertToDto(User user)
        {
            UserDTO userDto = new UserDTO();
            userDto.userId = user.Id;
            userDto.email = user.Email;
            userDto.name = user.Name;
            userDto.telNumber = user.TelNumber.ToString();
            userDto.description = user.Description;
            userDto.nationality = user.Nationality is null ? Nationality.None : (Nationality)Enum.Parse(typeof(Nationality), user.Nationality, true);
            userDto.city = user.City?.Name ?? "";
            userDto.cityPlaceId = user.CityId?.ToString() ?? "";
            userDto.profileImgUrl = _userImageService.GetUserProfileImageUrl(user.Id);
            return userDto;
        }
    }
}
