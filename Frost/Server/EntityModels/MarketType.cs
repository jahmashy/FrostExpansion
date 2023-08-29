using System;
using System.Collections.Generic;

namespace Frost.Server.EntityModels;

public partial class MarketType
{
    public int Id { get; set; }

    public string Type { get; set; } = null!;

    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();

    public virtual ICollection<SavedFilter> SavedFilters { get; set; } = new List<SavedFilter>();
}
