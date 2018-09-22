using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Backend.Models
{
    public class GamePlayerListResponse
    {
        public Game Game { get; set; }
        public List<Player> Players { get; set; }

        public GamePlayerListResponse(Game game, List<Player> players)
        {
            Game = game;
            Players = players;
        }
    }
}
