using Azure.Core;
using Frost.Server.Services.Interfaces;
using Frost.Shared.Models;
using Frost.Shared.Models.Enums;
using System.Runtime;

namespace Frost.Server.Services.ImageServices
{
    public interface IPropertyImageService
    {
        public (FileStatusCode, List<string>) GetPropertyImagesUrls(int offerId);
        public Task SaveImages(IEnumerable<IFormFile> files, string offerId);
        public FileStatusCode ValidateImages(IEnumerable<IFormFile> files, int minRequiredFiles, int maxAllowedFiles, long maxFileSize);
        public (FileStatusCode, List<byte[]>) GetPropertyImages(int offerId);
        public void CopyImages(int offerId, int TargetOfferId);
    }
    public class PropertyImageService : IPropertyImageService
    {
        private IConfiguration _configuration;
        public PropertyImageService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public (FileStatusCode, List<string>) GetPropertyImagesUrls(int offerId)
        {
            List<string> propertyImagesUrl = new();
            string dirPath = _configuration["Directories:PropertyImages"] + $"{offerId}";
            DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
            if (!dirInfo.Exists)
            {
                return (FileStatusCode.FileDoesNotExists, null);
            }
            FileInfo[] files = dirInfo.GetFiles();
            foreach (FileInfo file in files)
            {
                propertyImagesUrl.Add($"http://frosthousing.ddns.net/api/propertyimages/{offerId}/{file.Name}");
            }
            return (FileStatusCode.Success, propertyImagesUrl);
        }
        public async Task SaveImages(IEnumerable<IFormFile> files, string offerId)
        {

            string dirPath = _configuration["Directories:PropertyImages"] + $"{offerId}";

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
            foreach (var file in files)
            {

                string newExtension = ".webp";
                string newFileName = $"{Guid.NewGuid()}{newExtension}";

                var path = Path.Combine(dirPath, newFileName);

                using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
        }
        public FileStatusCode ValidateImages(IEnumerable<IFormFile> files, int minRequiredFiles, int maxAllowedFiles, long maxFileSize)
        {
            var filesCount = 0;
            foreach (var file in files)
            {
                filesCount++;
            }
            if (filesCount < minRequiredFiles)
                return FileStatusCode.NotEnoughFiles;

            if (filesCount > maxAllowedFiles)
                return FileStatusCode.TooManyFiles;
            foreach (var file in files)
            {
                if (file.Length > maxFileSize)
                    return FileStatusCode.FileIsTooBig;
                if (file == null || file.Length == 0)
                    return FileStatusCode.FileIsNull;

                string fileName = file.FileName;
                string extension = Path.GetExtension(fileName);

                string[] allowExtensions = { ".jpg", ".png", ".jpeg", ".webp" };

                if (!allowExtensions.Contains(extension.ToLower()))
                    return FileStatusCode.InvalidFileExtension;
            }
            return FileStatusCode.Success;

        }
        public (FileStatusCode, List<byte[]>) GetPropertyImages(int offerId)
        {
            List<byte[]> propertyImages = new();
            string dirPath = _configuration["Directories:PropertyImages"] + $"{offerId}";
            DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
            if (!dirInfo.Exists)
            {
                return (FileStatusCode.FileDoesNotExists, null);
            }
            FileInfo[] files = dirInfo.GetFiles();
            foreach (FileInfo file in files)
            {
                propertyImages.Add(File.ReadAllBytes(file.FullName));
            }
            return (FileStatusCode.Success, propertyImages);
        }
        public void CopyImages(int offerId, int TargetOfferId)
        {
            string sourceDirPath = _configuration["Directories:PropertyImages"] + $"{offerId}";
            string targetDirPath = _configuration["Directories:PropertyImages"] + $"{TargetOfferId}";

            DirectoryInfo sourceDirInfo = new DirectoryInfo(sourceDirPath);
            DirectoryInfo targetDirInfo = new DirectoryInfo(targetDirPath);
            if (!targetDirInfo.Exists)
            {
                Directory.CreateDirectory(targetDirPath);
            }
            else
            {
                FileInfo[] oldFiles = targetDirInfo.GetFiles();
                foreach (FileInfo oldFile in oldFiles)
                {
                    oldFile.Delete();
                }
            }
            FileInfo[] filesToCopy = sourceDirInfo.GetFiles();
            foreach (FileInfo file in filesToCopy)
            {
                string targetFilePath = Path.Combine(targetDirPath, file.Name);
                file.CopyTo(targetFilePath);
            }
        }
    }
}
