using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Backend.Models
{
    public class BRPlayer : Player
    {
        public BRPlayer(Player player)
        {
            //TODO: Make method
        }

        public bool IsEliminated { get; set; }
        public int LivesRemaining { get; set; }
        public bool IsInZone { get; set; }
    }
}
