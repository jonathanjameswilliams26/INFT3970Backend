using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Backend.Models
{
    public class Player
    {
        public int PlayerID { get; set; }
        public string Nickname { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string SelfieDataURL { get; set; }
        public int AmmoCount { get; set; }
        public int NumKills { get; set; }
        public int NumDeaths { get; set; }
        public int NumPhotosTaken { get; set; }
        public bool IsHost { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string ConnectionID { get; set; }
        public bool IsConnected { get; set; }
        public bool HasLeftGame { get; set; }
        public int GameID { get; set; }
        public Game Game { get; set; }
        public Player()
        {
            
        }

        public bool HasEmail()
        {
            if (string.IsNullOrWhiteSpace(Email))
                return false;
            else
                return true;
        }
    }
}
