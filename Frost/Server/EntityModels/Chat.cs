using System;
using System.Collections.Generic;

namespace Frost.Server.EntityModels;

public partial class Chat
{
    public int Id { get; set; }

    public bool GroupChat { get; set; }

    public virtual ICollection<ChatInvitation> ChatInvitations { get; set; } = new List<ChatInvitation>();

    public virtual ICollection<ChatRoom> ChatRooms { get; set; } = new List<ChatRoom>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
