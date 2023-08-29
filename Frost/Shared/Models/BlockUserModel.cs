using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Shared.Models
{
    public class BlockUserModel
    {
        [Required]
        public int userId { get; set; }
        [Required]
        public int targetUserId { get; set; }
    }
}
