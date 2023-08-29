using System.ComponentModel.DataAnnotations;

namespace Frost.Server.Models
{
    public class ChatInvitationModel
    {
        [Required]
        public string id { get; set; }
        [Required]
        public int chatID { get; set; }
        [Required]
        public DateTime expirationDate { get; set; }
    }
}
