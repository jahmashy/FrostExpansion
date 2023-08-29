using Frost.Shared.Models;
using Frost.Shared.Models.Enums;

namespace Frost.Server.Models
{
    public class MockUserModel
    {
        public int Id { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public Location location { get; set; }
        public Nationality nationality { get; set; } = 0;
        public string role { get; set; } = "User";
        public string refreshToken { get; set; }
        public DateTime refTokenExpDate { get; set; }
        public int telNumber { get; set; }
    }
}
