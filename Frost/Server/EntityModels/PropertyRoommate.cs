using System;
using System.Collections.Generic;

namespace Frost.Server.EntityModels;

public partial class PropertyRoommate
{
    public int Id { get; set; }

    public int PropertyId { get; set; }

    public int UserId { get; set; }

    public virtual Property Property { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
