using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Backend.Models
{
    public class Photo
    {
        public int PhotoID { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
        public string FilePath { get; set; }
        public DateTime? TimeTaken { get; set; }
        public DateTime? VotingFinishTime{get; set;}
        public int NumYesVotes { get; set; }
        public int NumNoVotes { get; set; }
        public bool IsVotingComplete { get; set; }
        public bool IsActive { get; set; }
        public int GameID { get; set; }
        public int TakenByPlayerID { get; set; }
        public int PhotoOfPlayerID { get; set; }

        public Photo() { }
    }
}
