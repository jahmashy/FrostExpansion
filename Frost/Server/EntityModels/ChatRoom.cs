using System;
using System.Collections.Generic;

namespace Frost.Server.EntityModels;

public partial class ChatRoom
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ChatId { get; set; }

    public virtual Chat Chat { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
