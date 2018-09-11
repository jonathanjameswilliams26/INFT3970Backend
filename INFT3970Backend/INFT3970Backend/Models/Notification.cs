using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Backend.Models
{
    public class Notification
    {
        public int NotificationID { get; set; }
        public string MessageText { get; set; }
        public string Type { get; set; }
        public bool IsRead { get; set; }
        public bool IsActive { get; set; }
        public int GameID { get; set; }
        public int PlayerID { get; set; }
        public Game Game { get; set; }
        public Player Player { get; set; }

        public Notification() { }
    }
}
