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
            PlayerID = player.PlayerID;
            Nickname = player.Nickname;
            Phone = player.Phone;
            Email = player.Email;
            SelfieDataURL = player.SelfieDataURL;
            AmmoCount = player.AmmoCount;
            NumKills = player.NumKills;
            NumDeaths = player.NumDeaths;
            NumPhotosTaken = player.NumPhotosTaken;
            GameID = player.GameID;
            IsHost = player.IsHost;
            IsVerified = player.IsVerified;
            IsActive = player.IsActive;
            IsDeleted = player.IsDeleted;
            ConnectionID = player.ConnectionID;
            HasLeftGame = player.HasLeftGame;
            base.Game = player.Game;
        }

        public bool IsEliminated { get; set; }
        public int LivesRemaining { get; set; }
        public bool IsInZone { get; set; }
        public new BRGame Game { get; set; }


        public bool IsAlive()
        {
            return LivesRemaining > 0;
        }
    }
}
