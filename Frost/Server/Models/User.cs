namespace Frost.Server.Models
{
    public class User
    {
        public int Id { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string role { get; set; } = "User";
        public string refreshToken { get; set; }
        public DateTime refTokenExpDate { get; set; }
        public string telNumber { get; set; }

        public User(int Id,string email, string password,string name,string telNumber)
        {
            this.Id = Id;
            this.email = email;
            this.password = password;
            this.name = name;
            this.telNumber = telNumber;
        }
    }
}
