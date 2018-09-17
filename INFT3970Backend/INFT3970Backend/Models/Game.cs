using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Backend.Models
{
    public class Game
    {
        public int GameID { get; set; }
        public string GameCode { get; set; }
        public int NumOfPlayers { get; set; }
        public string GameMode { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string GameState { get; set; }
        public bool IsJoinableAtAnytime { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public List<Player> Players { get; set; }
    }
}
