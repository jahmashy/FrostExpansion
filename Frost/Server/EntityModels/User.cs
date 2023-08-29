using System;
using System.Collections.Generic;

namespace Frost.Server.EntityModels;

public partial class User
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public int TelNumber { get; set; }

    public string Password { get; set; } = null!;

    public string PasswordSalt { get; set; } = null!;

    public int RoleId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Nationality { get; set; }

    public int? CityId { get; set; }

    public string RefreshToken { get; set; } = null!;

    public DateTime RefreshTokenExpirationDate { get; set; }

    public int? SavedFiltersId { get; set; }

    public bool NotifyAboutNewOffers { get; set; }

    public bool NotifyAboutNewMessages { get; set; }

    public bool NotifyAboutChangedLoginDetails { get; set; }

    public bool NotifyAboutExpiringOffers { get; set; }

    public virtual ICollection<BlockedCommunication> BlockedCommunicationUser1s { get; set; } = new List<BlockedCommunication>();

    public virtual ICollection<BlockedCommunication> BlockedCommunicationUser2s { get; set; } = new List<BlockedCommunication>();

    public virtual ICollection<ChatRoom> ChatRooms { get; set; } = new List<ChatRoom>();

    public virtual City? City { get; set; }

    public virtual ICollection<FollowedOffer> FollowedOffers { get; set; } = new List<FollowedOffer>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();

    public virtual ICollection<PropertyRoommate> PropertyRoommates { get; set; } = new List<PropertyRoommate>();

    public virtual Role Role { get; set; } = null!;

    public virtual SavedFilter? SavedFilters { get; set; }

    public virtual ICollection<ViewCount> ViewCounts { get; set; } = new List<ViewCount>();
}
