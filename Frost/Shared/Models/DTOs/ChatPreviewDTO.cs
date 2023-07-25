
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Shared.Models.DTOs
{
    public class ChatPreviewDTO
    {
        public int Id { get; set; }
        public List<UserDto> participants { get; set; }
        public List<Message> messages { get; set; }
    }
}
