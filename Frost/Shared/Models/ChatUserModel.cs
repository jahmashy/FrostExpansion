using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Shared.Models
{
    public class ChatUserModel
    {
        [Required]
        public int chatId { get; set; }
        [Required]
        public int userId { get; set; }
    }
}
