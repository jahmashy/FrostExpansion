using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Shared.Models
{
    public class Location
    {
        [Required(ErrorMessageResourceName = "FieldRequiredError", ErrorMessageResourceType = typeof(Resources.Errors))]
        public string? cityName { get; set; } = "";
        [Required(ErrorMessageResourceName = "FieldRequiredError", ErrorMessageResourceType = typeof(Resources.Errors))]
        public string? cityPlaceId { get; set; } = "";
        public string? administrativeAreaName { get; set; } = "";
        public string? administrativeAreaPlaceId { get; set; } = "";
        public string? districtName { get; set; } = "";
        public string? districtPlaceId { get; set; } = "";
    }
}
