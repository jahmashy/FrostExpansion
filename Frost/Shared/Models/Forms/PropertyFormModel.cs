using Frost.Shared.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Shared.Models.Forms
{
    public class PropertyFormModel
    {

        [Required(ErrorMessageResourceName = "FieldRequiredError", ErrorMessageResourceType = typeof(Resources.Errors))]
        [MaxLength(60, ErrorMessageResourceName = "TitleMaxLengthError", ErrorMessageResourceType = typeof(Resources.Errors))]
        public string Title { get; set; }

        [Required(ErrorMessageResourceName = "FieldRequiredError", ErrorMessageResourceType = typeof(Resources.Errors))]
        [MaxLength(2000, ErrorMessageResourceName = "DescriptionMaxLengthError", ErrorMessageResourceType = typeof(Resources.Errors))]
        public string Description { get; set; }

        [Required(ErrorMessageResourceName = "FieldRequiredError", ErrorMessageResourceType = typeof(Resources.Errors))]
        [Range(1, double.MaxValue, ErrorMessageResourceName = "PriceRangeError", ErrorMessageResourceType = typeof(Resources.Errors))]
        public double? Price { get; set; }

        [Required(ErrorMessageResourceName = "FieldRequiredError", ErrorMessageResourceType = typeof(Resources.Errors))]
        [Range(1, double.MaxValue, ErrorMessageResourceName = "SurfaceRangeError", ErrorMessageResourceType = typeof(Resources.Errors))]
        public double? Surface { get; set; }

        [Required(ErrorMessageResourceName = "FieldRequiredError", ErrorMessageResourceType = typeof(Resources.Errors))]
        public PropertyType PropertyType { get; set; }

        [Required(ErrorMessageResourceName = "FieldRequiredError", ErrorMessageResourceType = typeof(Resources.Errors))]
        [Range(1, 100, ErrorMessageResourceName = "RoomsNumberRangeError", ErrorMessageResourceType = typeof(Resources.Errors))]
        public int? RoomsNumber { get; set; }

        [Required(ErrorMessageResourceName = "FieldRequiredError", ErrorMessageResourceType = typeof(Resources.Errors))]
        [Range(1900, int.MaxValue, ErrorMessageResourceName = "YearRangeError", ErrorMessageResourceType = typeof(Resources.Errors))]
        public int? ConstructionYear { get; set; }

        [Required(ErrorMessageResourceName = "FieldRequiredError", ErrorMessageResourceType = typeof(Resources.Errors))]
        [Range(1, 100, ErrorMessageResourceName = "FloorRangeError", ErrorMessageResourceType = typeof(Resources.Errors))]
        public int? Floor { get; set; }

        [Required(ErrorMessageResourceName = "FieldRequiredError", ErrorMessageResourceType = typeof(Resources.Errors))]
        public Location Location { get; set; }

        [Required(ErrorMessageResourceName = "FieldRequiredError", ErrorMessageResourceType = typeof(Resources.Errors))]
        public OfferType offerType { get; set; }

        [Required(ErrorMessageResourceName = "FieldRequiredError", ErrorMessageResourceType = typeof(Resources.Errors))]
        public MarketType marketType { get; set; }

        [Required(ErrorMessageResourceName = "FieldRequiredError", ErrorMessageResourceType = typeof(Resources.Errors))]
        public bool RoommatesAllowed { get; set; } = false;

    }
}
