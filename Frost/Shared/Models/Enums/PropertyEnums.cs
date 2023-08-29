using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Shared.Models.Enums
{
    public enum PropertyTypeEnum
    {
        Flat,
        House,
        Plot,
        Establishment,
        Garage,
        Room
    }
    public enum OfferTypeEnum
    {
        Sell,
        Rent
    }
    public enum RoommatesEnum
    {
        Any,
        Excluded,
        Included
    }
    public enum MarketTypeEnum
    {
        Any,
        Primary,
        Secondary
    }
}
