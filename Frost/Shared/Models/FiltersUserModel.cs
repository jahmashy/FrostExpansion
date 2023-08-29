using Frost.Shared.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Shared.Models
{
    public class FiltersUserModel
    {
        public PropertyFiltersDTO filtersDto { get; set; }
        public int userId { get; set; }
    }
}
