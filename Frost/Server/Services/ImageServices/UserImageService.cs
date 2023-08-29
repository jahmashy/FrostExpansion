using Frost.Server.Models;
using Frost.Shared.Models.Enums;
using System.Runtime;

namespace Frost.Server.Services.ImageServices
{
    public interface IUserImageService
    {
        public Task SaveImageAsync(IFormFile file, string userId);
        public FileStatusCode ValidateImage(IFormFile file, long maxFileSize, string[] allowExtensions);
        public (FileStatusCode, byte[]?) GetUserProfileImage(int userId);
        public string GetUserProfileImageUrl(int userId);
        public void DeleteUserProfileImage(int userId);
    }
    public class UserImageService : IUserImageService
    {
        private IConfiguration _configuration;
        public UserImageService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task SaveImageAsync(IFormFile file, string userId)
        {
            string dirPath = _configuration["Directories:UserImages"] + $"{userId}";

            DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
            if (!dirInfo.Exists)
            {
                Directory.CreateDirectory(dirPath);
            }
            else
            {
                FileInfo[] oldFiles = dirInfo.GetFiles();
                foreach (FileInfo oldFile in oldFiles)
                {
                    oldFile.Delete();
                }
            }

            string newExtension = ".webp";
            string newFileName = $"{Guid.NewGuid()}{newExtension}";

            var path = Path.Combine(dirPath, newFileName);

            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                await file.CopyToAsync(fileStream);
            }
        }

        public FileStatusCode ValidateImage(IFormFile file, long maxFileSize, string[] allowExtensions)
        {
            if (file.Length > maxFileSize)
                return FileStatusCode.FileIsTooBig;
            if (file == null || file.Length == 0)
                return FileStatusCode.FileIsNull;

            string fileName = file.FileName;
            string extension = Path.GetExtension(fileName);

            if (!allowExtensions.Contains(extension.ToLower()))
                return FileStatusCode.InvalidFileExtension;

            return FileStatusCode.Success;
        }
        public (FileStatusCode, byte[]?) GetUserProfileImage(int userId)
        {
            string dirPath = _configuration["Directories:UserImages"] + $"{userId}";

            byte[] userProfileImage = null;
            DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
            if (!dirInfo.Exists)
            {
                return (FileStatusCode.FileDoesNotExists, null);
            }
            FileInfo[] files = dirInfo.GetFiles();
            foreach (FileInfo file in files)
            {
                userProfileImage = File.ReadAllBytes(file.FullName);
            }
            if (userProfileImage is null)
                return (FileStatusCode.FileDoesNotExists, null);
            return (FileStatusCode.Success, userProfileImage);
        }
        public string GetUserProfileImageUrl(int userId)
        {
            string UserProfileImageUrl = "";
            string dirPath = _configuration["Directories:UserImages"] + $"{userId}";
            DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
            if (!dirInfo.Exists)
                return UserProfileImageUrl;
            FileInfo[] files = dirInfo.GetFiles();
            if (files.Length == 0)
                return UserProfileImageUrl;
            UserProfileImageUrl = $"http://frosthousing.ddns.net/api/userImage/{userId}";
            return UserProfileImageUrl;
        }
        public void DeleteUserProfileImage(int userId)
        {
            string dirPath = _configuration["Directories:UserImages"] + $"{userId}";
            DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
            FileInfo[] oldFiles = dirInfo.GetFiles();
            foreach (FileInfo oldFile in oldFiles)
            {
                oldFile.Delete();
            }
        }
    }
}
