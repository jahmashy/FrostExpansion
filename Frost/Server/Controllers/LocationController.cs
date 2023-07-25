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
        [HttpGet("cities")]
        public async Task<IActionResult> GetCities(string? userInput)
        {
            var cities = await googlePlacesApi.GetCitiesAsync(userInput);
            return Ok(cities);
        }
        [HttpGet("districts")]
        public async Task<IActionResult> GetDistricts(string? userInput, string city)
        {
            var districts = await googlePlacesApi.GetDistrictsAsync(userInput, city);
            return Ok(districts);
        }

    }
}
