using Frost.Shared.Models.DTOs;
using Frost.Shared.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Shared.Models
{
    public class PropertyOffer
    {
        public PropertyOffer(int offerId, string title, double price, string description, Property property, OfferType offerType, MarketType marketType, bool roommates)
        {
            this.offerId = offerId;
            this.title = title;
            this.price = price;
            this.description = description;
            this.property = property;
            this.offerType = offerType;
            this.marketType = marketType;
            this.roommates = roommates;
        }

        public int offerId { get; set; }
        public string title { get; set; }
        
        public double price { get; set; }
        public string description { get; set; }
        public Property property { get; set; }

        public OfferType offerType { get; set; }
        
        public MarketType marketType { get; set; }

        public bool roommates { get; set; }
    }
}
