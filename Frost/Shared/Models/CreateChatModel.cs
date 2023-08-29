using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Shared.Models
{
    public class CreateChatModel
    {
        [Required]
        public int user_id { get; set; }
        [Required]
        public int targetUser_id { get; set;}

        public string? firstMessage { get; set; }
    }
}
