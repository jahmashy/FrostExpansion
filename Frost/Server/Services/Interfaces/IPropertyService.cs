using Frost.Shared.Models;
using Frost.Shared.Models.DTOs;
using Frost.Shared.Models.Enums;
using Frost.Shared.Models.Forms;

namespace Frost.Server.Services.Interfaces
{
    public interface IPropertyService
    {
        public Task<IEnumerable<PropertyDetailsDTO>> GetPromotedPropertiesAsync();
        public Task<PropertyDetailsDTO> GetPropertyDetailsAsync(int offerId);
        public Task<IEnumerable<PropertyDetailsDTO>> GetUserPropertiesAsync(string email);
        public Task<(bool, int)> CreateProperty(PropertyFormModel property, string email);

    }
}
