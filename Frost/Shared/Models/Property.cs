using Frost.Shared.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Shared.Models
{
    public class Property
    {
        public Property(double surface, PropertyType propertyType, int roomsNumber, int? constructionYear, int floor, Location location, List<string> propertyImagesUrls)
        {
            this.surface = surface;
            this.propertyType = propertyType;
            this.roomsNumber = roomsNumber;
            this.constructionYear = constructionYear;
            this.floor = floor;
            this.location = location;
            this.propertyImagesUrls = propertyImagesUrls;
        }

        public double surface { get; set; }
        public PropertyType propertyType { get; set; }
        public int roomsNumber { get; set; }
        public int? constructionYear { get; set; }
        public int floor { get; set; }
        public Location location { get; set; }
        public List<string> propertyImagesUrls { get; set; }
    }
}
