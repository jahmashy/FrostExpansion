using System;
using System.Collections.Generic;

namespace Frost.Server.EntityModels;

public partial class City
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string GoogleId { get; set; } = null!;

    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();

    public virtual ICollection<SavedFilter> SavedFilters { get; set; } = new List<SavedFilter>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
