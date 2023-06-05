using Frost.Shared.Models;

namespace Frost.Server.Services.Interfaces
{
    public interface IGooglePlacesApiService
    {
        public Task<IEnumerable<Location>> GetLocationsAsync(string? userInput);
    }
}
