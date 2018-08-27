using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Backend.Models
{
    public class Game
    {
        public int GameID { get; set; }
        public string GameName { get; set; }
        public string GameCode { get; set; }
        public int NumOfPlayers { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsComplete { get; set; }
        public bool IsActive { get; set; }
        public Dictionary<int, Player> Players { get; set; }

    }
}
