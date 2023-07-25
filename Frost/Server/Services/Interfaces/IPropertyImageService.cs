using Frost.Shared.Models;
using Frost.Shared.Models.Enums;

namespace Frost.Server.Services.Interfaces
{
    public interface IPropertyImageService
    {
        public (FileStatusCode, List<string>) GetPropertyImagesUrls(int offerId);
        public Task SaveImages(IEnumerable<IFormFile> files, string offerId);
        public FileStatusCode ValidateImages(IEnumerable<IFormFile> files, int minRequiredFiles, int maxAllowedFiles, long maxFileSize);
    }
}
