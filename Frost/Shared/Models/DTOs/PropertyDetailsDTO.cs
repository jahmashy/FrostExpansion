using Frost.Shared.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Shared.Models.DTOs
{
    public class PropertyDetailsDTO
    {
        public int OfferId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Price { get; set; }
        public string Surface { get; set; }
        public PropertyType PropertyType { get; set; }
        public string RoomsNumber { get; set; }
        public string ConstructionYear { get; set; }
        public string Floor { get; set; }
        public Location Location { get; set; }
        public List<string>? propertyImagesUrls { get; set; }
        public OfferType offerType { get; set; }

        public MarketType marketType { get; set; }

        public bool RoommatesAllowed { get; set; }
        public UserDto User { get; set; }

        public List<UserDto> RoommatesList { get; set; }

    }
}
