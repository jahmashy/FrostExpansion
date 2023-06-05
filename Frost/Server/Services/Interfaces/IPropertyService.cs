using Frost.Shared.Models;

namespace Frost.Server.Services.Interfaces
{
    public interface IPropertyService
    {
        public Task<IEnumerable<PropertyOffer>> GetPromotedPropertiesAsync();
    }
}
