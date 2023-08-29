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
        public PropertyTypeEnum propertyType { get; set; }
        public string RoomsNumber { get; set; }
        public string ConstructionYear { get; set; }
        public string Floor { get; set; }
        public Location location { get; set; }
        public List<string>? propertyImagesUrls { get; set; }
        public OfferTypeEnum offerType { get; set; }

        public MarketTypeEnum marketType { get; set; }
        public bool RoommatesAllowed { get; set; }
        public UserDTO user { get; set; }

        public List<UserDTO> RoommatesList { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool PriceLowerThanAverage { get; set; }
        public double PercentageDiff { get; set; }

    }
}
