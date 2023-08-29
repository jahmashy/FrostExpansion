using System.ComponentModel.DataAnnotations;

namespace Frost.Shared.Models
{
    public class MessageModel
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
        public int chat_Id { get; set; }
    }
}
