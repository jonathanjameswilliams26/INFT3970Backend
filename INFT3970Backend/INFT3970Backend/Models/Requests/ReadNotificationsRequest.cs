using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Backend.Models
{
    public class ReadNotificationsRequest
    {
        public string PlayerID { get; set; }
        public string[] NotificationArray { get; set; }
    }
}
