using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Shared.Models
{
    public class NotificationModel
    {
        public bool NotifyAboutNewOffers { get; set; }
        public bool NotifyAboutExpiringOffers { get; set; }
        public bool NotifyAboutNewMessages { get; set; }
        public bool NotifyAboutChangedLoginDetails { get; set; }
    }
}
