using Frost.Server.Services.Interfaces;
using Frost.Shared.Models;
using Frost.Shared.Models.Enums;
using System.Runtime;

namespace Frost.Server.Services
{
    public class PropertyImageService : IPropertyImageService
    {
        public (FileStatusCode,List<string>) GetPropertyImagesUrls(int offerId)
        {
            List<string> propertyImagesUrl = new();
            string dirPath = $"Resources/PropertyImages/{offerId}";
            DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
            if (!dirInfo.Exists)
            {
                return(FileStatusCode.FileDoesNotExists,null);
            }
            FileInfo[] files = dirInfo.GetFiles();
            foreach (FileInfo file in files)
            {
                propertyImagesUrl.Add($"https://localhost:44350/api/propertyimages/{offerId}/{file.Name}");
            }
            return (FileStatusCode.Success,propertyImagesUrl);
        }
        public async Task SaveImages(IEnumerable<IFormFile> files, string offerId)
        {

            string dirPath = $"Resources/PropertyImages/{offerId}";

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
                string fileName = file.FileName;
                string extension = Path.GetExtension(fileName);

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
    }
}
