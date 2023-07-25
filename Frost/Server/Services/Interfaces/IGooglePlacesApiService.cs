using Frost.Shared.Models;

namespace Frost.Server.Services.Interfaces
{
    public interface IGooglePlacesApiService
    {
        public Task<IEnumerable<Location>> GetCitiesAsync(string? userInput);
        public Task<IEnumerable<Location>> GetDistrictsAsync(string? userInput,string? city);
    }
}
