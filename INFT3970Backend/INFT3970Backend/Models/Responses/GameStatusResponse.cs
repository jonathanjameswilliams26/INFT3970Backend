using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Backend.Models.Responses
{
    public class GameStatusResponse
    {
        public string GameState { get; set; }
        public bool HasVotesToComplete { get; set; }
        public bool HasNotifications { get; set; }
        public Player Player { get; set; }


        public GameStatusResponse(string gameState, bool hasVotesToComplete, bool hasNotifications, Player player)
        {
            GameState = gameState;
            HasVotesToComplete = hasVotesToComplete;
            HasNotifications = hasNotifications;
            Player = player;
        }
    }
}
