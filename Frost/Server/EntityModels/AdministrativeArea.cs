using System;
using System.Collections.Generic;

namespace Frost.Server.EntityModels;

public partial class AdministrativeArea
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string GoogleId { get; set; } = null!;

    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();
}
