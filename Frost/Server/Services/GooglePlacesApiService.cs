using Frost.Server.Services.Interfaces;
using Frost.Shared.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Frost.Server.Services
{

    public class GooglePlacesApiService : IGooglePlacesApiService
    {

        private IConfiguration _configuration;
        public GooglePlacesApiService(IConfiguration configuration) { 
            this._configuration = configuration;
        }
        public async Task<IEnumerable<Location>> GetCitiesAsync(string? userInput)
        {
            List<Location> locations = new List<Location>();
            if(String.IsNullOrWhiteSpace(userInput))
                return locations;

            string apiKey = _configuration["GoogleApi:ApiKey"];
            string apiUrl = $"https://maps.googleapis.com/maps/api/place/autocomplete/json?input={userInput}&types=(cities)&components=country:pl&language=pl&key={apiKey}";

            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(apiUrl);
                if (response ==  null || !response.IsSuccessStatusCode)
                    return locations;

                var result = await response.Content.ReadAsStringAsync();
                var content = JObject.Parse(result);
                JArray jArray = (JArray)content["predictions"];
                foreach (var prediction in jArray)
                {
                    locations.Add(
                        new Location(){
                            cityName = prediction["structured_formatting"]["main_text"].ToString(),
                            cityPlaceId = prediction["place_id"].ToString()
                    });
                }

            }
            
            return locations;
        }

        public async Task<IEnumerable<Location>> GetDistrictsAsync(string? userInput, string? city)
        {
            List<Location> locations = new List<Location>();
            if (String.IsNullOrWhiteSpace(userInput) || String.IsNullOrWhiteSpace(city))
                return locations;

            string apiKey = _configuration["GoogleApi:ApiKey"];
            string apiUrl = $"https://maps.googleapis.com/maps/api/place/autocomplete/json?input={userInput}&types=neighborhood|sublocality&key={apiKey}&components=country:pl&language=pl";

            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(apiUrl);
                if (response == null || !response.IsSuccessStatusCode)
                    return locations;

                var result = await response.Content.ReadAsStringAsync();
                var content = JObject.Parse(result);
                JArray jArray = (JArray)content["predictions"];
                foreach (var prediction in jArray)
                {
                    var receivedCity = prediction["terms"][1]["value"].ToString();
                    if (receivedCity.Equals(city))
                    {
                        locations.Add(
                            new Location()
                            {
                                districtName = prediction["structured_formatting"]["main_text"].ToString(),
                                districtPlaceId = prediction["place_id"].ToString()
                            });
                    }

                }

            }

            return locations;
        }
    }
}
