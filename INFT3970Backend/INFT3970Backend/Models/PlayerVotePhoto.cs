using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Backend.Models
{
    public class PlayerVotePhoto
    {
        public int VoteID { get; set; }
        public bool? IsPhotoSuccessful { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int PlayerID { get; set; }
        public int PhotoID { get; set; }
        public Player Player { get; set; }
        public Photo Photo { get; set; }

        public PlayerVotePhoto() { }
    }
}
