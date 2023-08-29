using System;
using System.Collections.Generic;

namespace Frost.Server.EntityModels;

public partial class SavedFilter
{
    public int Id { get; set; }

    public double? MinPrice { get; set; }

    public double? MaxPrice { get; set; }

    public float? MinSurface { get; set; }

    public float? MaxSurface { get; set; }

    public int? MinFloor { get; set; }

    public int? MaxFloor { get; set; }

    public int? MinRooms { get; set; }

    public int? MaxRooms { get; set; }

    public int? MinConstructionYear { get; set; }

    public int? MaxConstructionYear { get; set; }

    public bool? Roommates { get; set; }

    public int? CityId { get; set; }

    public int? MarketTypeId { get; set; }

    public int PropertyTypeId { get; set; }

    public int OfferTypeId { get; set; }

    public double? MinMeterPrice { get; set; }

    public double? MaxMeterPrice { get; set; }

    public virtual City? City { get; set; }

    public virtual MarketType? MarketType { get; set; }

    public virtual OfferType OfferType { get; set; } = null!;

    public virtual PropertyType PropertyType { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
