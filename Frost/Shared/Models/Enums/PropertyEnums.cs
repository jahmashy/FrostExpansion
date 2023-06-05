using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Shared.Models.Enums
{
    public enum PropertyType
    {
        Flat,
        House,
        Plot,
        Establishment,
        Garage,
        Room
    }
    public enum OfferType
    {
        Buy,
        Rent
    }
    public enum Roommates
    {
        Any,
        Excluded,
        Included
    }
    public enum MarketType
    {
        Any,
        Primary,
        Secondary
    }
    public enum ConstructionType
    {
        Any,
        HighRise,
        Tenement,
        RowHouse
    }
}
