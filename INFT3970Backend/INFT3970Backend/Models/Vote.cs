using INFT3970Backend.Models.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Backend.Models
{
    public class Vote
    {
        private int voteID;
        private int playerID;
        private int photoID;

        public int VoteID
        {
            get { return voteID; }
            set
            {
                var errorMessage = ErrorMessages.EM_PHOTO_MODELINVALID
                                    + "VoteID must be atleast 100000.";

                if (value == -1 || value >= 100000)
                    voteID = value;

                else
                    throw new InvalidModelException(errorMessage, ErrorCodes.EC_PLAYER_ID);
            }
        }



        public bool? IsPhotoSuccessful { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int PlayerID
        {
            get { return playerID; }
            set
            {
                var errorMessage = ErrorMessages.EM_PHOTO_MODELINVALID
                                    + "PlayerID must be atleast 100000.";

                if (value == -1 || value >= 100000)
                    playerID = value;

                else
                    throw new InvalidModelException(errorMessage, ErrorCodes.EC_PLAYER_ID);
            }
        }



        public int PhotoID
        {
            get { return photoID; }
            set
            {
                var errorMessage = ErrorMessages.EM_PHOTO_MODELINVALID
                                    + "PhotoID must be atleast 100000.";

                if (value == -1 || value >= 100000)
                    photoID = value;

                else
                    throw new InvalidModelException(errorMessage, ErrorCodes.EC_PLAYER_ID);
            }
        }



        public Player Player { get; set; }
        public Photo Photo { get; set; }

        public Vote()
        {
            VoteID = -1;
            PlayerID = -1;
            PhotoID = -1;
        }

        public Vote(int voteID, bool? isPhotoSuccessful, int playerID)
        {
            PhotoID = -1;
            VoteID = voteID;
            IsPhotoSuccessful = isPhotoSuccessful;
            PlayerID = playerID;
        }

        public Vote(int voteID, string isPhotoSuccessful, int playerID)
        {
            PhotoID = -1;
            VoteID = voteID;
            PlayerID = playerID;
            try
            {
                IsPhotoSuccessful = bool.Parse(isPhotoSuccessful);
            }
            catch(FormatException)
            {
                string msg = ErrorMessages.EM_PHOTO_MODELINVALID
                                    + "IsPhotoSuccessful can only be true or false, received " + isPhotoSuccessful;
                throw new InvalidModelException(msg, 0);
            }
        }
    }
}
