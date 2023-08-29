using System;
using System.Collections.Generic;

namespace Frost.Server.EntityModels;

public partial class Property
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public double Price { get; set; }

    public float Surface { get; set; }

    public int RoomsNumber { get; set; }

    public int ConstructionYear { get; set; }

    public int Floor { get; set; }

    public bool RoommatesAllowed { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime ExpirationDate { get; set; }

    public int CityId { get; set; }

    public int? AdministrativeAreaId { get; set; }

    public int? SublocalityId { get; set; }

    public int UserId { get; set; }

    public int MarketTypeId { get; set; }

    public int PropertyTypeId { get; set; }

    public int OfferTypeId { get; set; }

    public bool IsTemplate { get; set; }

    public virtual AdministrativeArea? AdministrativeArea { get; set; }

    public virtual City City { get; set; } = null!;

    public virtual ICollection<FollowedOffer> FollowedOffers { get; set; } = new List<FollowedOffer>();

    public virtual MarketType MarketType { get; set; } = null!;

    public virtual OfferType OfferType { get; set; } = null!;

    public virtual ICollection<PropertyRoommate> PropertyRoommates { get; set; } = new List<PropertyRoommate>();

    public virtual PropertyType PropertyType { get; set; } = null!;

    public virtual Sublocality? Sublocality { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual ICollection<ViewCount> ViewCounts { get; set; } = new List<ViewCount>();
}
