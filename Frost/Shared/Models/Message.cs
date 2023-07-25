using System.ComponentModel.DataAnnotations;

namespace Frost.Shared.Models
{
    public class Message
    {
        [Required]
        [MaxLength(2000, ErrorMessageResourceName = "MessageMaxLengthError", ErrorMessageResourceType = typeof(Resources.Errors))]
        public string content { get; set; }
        [Required]
        public DateTime sendDate { get; set; }
        [Required]
        public int user_Id { get; set; }
        [Required]
        public string userName { get; set; }
        [Required]
        public int chatroom_Id { get; set; }
    }
}
