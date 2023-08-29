using Frost.Shared.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Shared.Models.Forms
{
    public class UserDetailsFormModel
    {
        [Required(ErrorMessageResourceName = "NameRequiredError", ErrorMessageResourceType = typeof(Resources.Errors))]
        [MaxLength(30,ErrorMessageResourceName ="NameTooLongError", ErrorMessageResourceType = typeof(Resources.Errors))]
        public string name { get; set; }
        [MaxLength(500, ErrorMessageResourceName = "UserDescriptionTooLongError", ErrorMessageResourceType = typeof(Resources.Errors))]
        public string? description { get; set; }
        public string cityName { get; set; } = "";
        public string cityPlaceId { get; set; } = "";
        [Required(ErrorMessageResourceName = "NationalityRequiredError", ErrorMessageResourceType = typeof(Resources.Errors))]
        public Nationality nationality { get; set; }
    }
}
