using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Shared.Models.DTOs
{
    public class JwtDTO
    {
        [Required]
        public string token { get; set; }
        [Required]
        public string refreshToken { get; set; }

        public JwtDTO(string token, string refreshToken)
        {
            this.token = token;
            this.refreshToken = refreshToken;
        }
    }
}
