using Frost.Shared.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Shared.Models.DTOs
{
    public class PropertyFiltersDTO
    {
        public PropertyTypeEnum propertyType { get; set; }
        public OfferTypeEnum offerType { get; set; }
        public MarketTypeEnum? marketType { get; set; }
        public string? cityName { get; set; }
        public string? cityPlaceId { get; set; }

        public double? minPrice { get; set; }
        public double? maxPrice { get; set; }
        public float? minSurface { get; set; }
        public float? maxSurface { get; set; }
        public int? minRoomsNumber { get; set; }
        public int? maxRoomsNumber { get; set; }
        public double? minMeterPrice { get; set; }
        public double? maxMeterPrice { get; set;}
        public RoommatesEnum? roommates { get; set; }
        public int? minFloor { get; set; }
        public int? maxFloor { get; set; }
        public int? minConstructionYear { get; set; }
        public int? maxConstructionYear { get; set; }


    }
}
