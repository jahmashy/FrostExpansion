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
        public PropertyType propertyType { get; set; }
        public OfferType transactionType { get; set; }
        public Location location { get; set; }
        public int? minPrice { get; set; }
        public int? maxPrice { get; set; }
        public double? minSurface { get; set; }
        public double? maxSurface { get; set; }
        public int? roomsNumber { get; set; }

        public int? minMeterPrice { get; set; }

        public int? maxMeterPrice { get; set;}
        public ConstructionType buildingType { get; set; }

        public Roommates roommates { get; set; }

        [Range(1,53)]
        public int  ? floor { get; set; }
        public int minConstructionYear { get; set; }

        public int maxConstructionYear { get; set; }
        public MarketType _MarketType { get; set; }

    }
}
