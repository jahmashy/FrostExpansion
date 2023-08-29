using System;
using System.Collections.Generic;

namespace Frost.Server.EntityModels;

public partial class ChatInvitation
{
    public string Id { get; set; } = null!;

    public DateTime ExpirationDate { get; set; }

    public int ChatId { get; set; }

    public virtual Chat Chat { get; set; } = null!;
}
