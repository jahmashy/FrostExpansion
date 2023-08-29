using Frost.Shared.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Shared.Models
{
    public class SearchResult
    {
        public IEnumerable<PropertyDetailsDTO> searchedOffers { get; set; }
        public int remainingResults { get; set; }
    }
}
