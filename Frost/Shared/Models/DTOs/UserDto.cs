using Frost.Shared.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Shared.Models.DTOs
{
    public class UserDTO
    {
        public int userId { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string? description { get; set; }
        public string? city { get; set; }
        public string? cityPlaceId { get; set; }
        public Nationality? nationality { get; set; }
        public string telNumber { get; set; }
        public string? profileImgUrl { get; set; }
    }
}
