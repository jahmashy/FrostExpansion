using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Shared.Models
{
    public class Location
    {
        public Location(string cityName, string cityPlaceId, string administrativeArea, string administrativeAreaPlaceId, string sublocality, string sublocalityPlaceId)
        {
            this.cityName = cityName;
            this.cityPlaceId = cityPlaceId;
            this.administrativeAreaName = administrativeArea;
            this.administrativeAreaPlaceId = administrativeAreaPlaceId;
            this.sublocalityName = sublocality;
            this.sublocalityPlaceId = sublocalityPlaceId;
        }

        public string cityName { get; set; }
        public string cityPlaceId { get; set;}
        public string? administrativeAreaName { get; set; }
        public string? administrativeAreaPlaceId { get; set; }
        public string? sublocalityName { get; set; }
        public string? sublocalityPlaceId { get; set; }
    }
}
