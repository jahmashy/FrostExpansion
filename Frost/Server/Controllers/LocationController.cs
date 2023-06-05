using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Frost.Server.Services;
using Frost.Server.Services.Interfaces;

namespace Frost.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private IGooglePlacesApiService googlePlacesApi;
        public LocationController(IGooglePlacesApiService googlePlacesApi) {
            this.googlePlacesApi = googlePlacesApi;
        }
        [HttpGet]
        public async Task<IActionResult> GetLocations(string? userInput)
        {
            var locations = await googlePlacesApi.GetLocationsAsync(userInput);
            return Ok(locations);
        }
    }
}
