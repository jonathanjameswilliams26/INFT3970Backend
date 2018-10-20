using INFT3970Backend.Models.Errors;
using System;

namespace INFT3970Backend.Models
{
    public class Vote
    {
        //Private backing stores for public properties
        private int voteID;
        private int playerID;
        private int photoID;

        public int VoteID
        {
            get { return voteID; }
            set
            {
                var errorMessage = "VoteID must be atleast 100000.";

                if (value == -1 || value >= 100000)
                    voteID = value;

                else
                    throw new InvalidModelException(errorMessage, ErrorCodes.MODELINVALID_VOTE);
            }
        }


        
        public int PlayerID
        {
            get { return playerID; }
            set
            {
                var errorMessage = "PlayerID must be atleast 100000.";

                if (value == -1 || value >= 100000)
                    playerID = value;

                else
                    throw new InvalidModelException(errorMessage, ErrorCodes.MODELINVALID_VOTE);
            }
        }



        public int PhotoID
        {
            get { return photoID; }
            set
            {
                var errorMessage = "PhotoID must be atleast 100000.";

                if (value == -1 || value >= 100000)
                    photoID = value;

                else
                    throw new InvalidModelException(errorMessage, ErrorCodes.MODELINVALID_VOTE);
            }
        }



        public bool? IsPhotoSuccessful { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public Player Player { get; set; }
        public Photo Photo { get; set; }





        /// <summary>
        /// Creates a Vote with the default values
        /// </summary>
        public Vote()
        {
            VoteID = -1;
            PlayerID = -1;
            PhotoID = -1;
            IsActive = true;
            IsDeleted = false;
            IsPhotoSuccessful = null;
        }




        /// <summary>
        /// Creates a Vote with the default values and sets the following properties.
        /// </summary>
        /// <param name="voteID">The ID of the vote</param>
        /// <param name="isPhotoSuccessful">The decision of the vote, true or false/param>
        /// <param name="playerID">The ID of the player who is casting the vote.</param>
        public Vote(int voteID, bool? isPhotoSuccessful, int playerID) : this()
        {
            VoteID = voteID;
            IsPhotoSuccessful = isPhotoSuccessful;
            PlayerID = playerID;
        }




        /// <summary>
        /// Creates a Vote with the default values and sets the following properties.
        /// </summary>
        /// <param name="voteID">The ID of the vote</param>
        /// <param name="isPhotoSuccessful">The decision of the vote, true or false/param>
        /// <param name="playerID">The ID of the player who is casting the vote.</param>
        public Vote(int voteID, string isPhotoSuccessful, int playerID) : this()
        {
            VoteID = voteID;
            PlayerID = playerID;
            try
            {
                IsPhotoSuccessful = bool.Parse(isPhotoSuccessful);
            }
            catch(FormatException)
            {
                var errorMessage = "IsPhotoSuccessful can only be true or false, received " + isPhotoSuccessful;
                throw new InvalidModelException(errorMessage, ErrorCodes.MODELINVALID_VOTE);
            }
        }



        public void Compress()
        {
            if (Player != null)
                Player.Compress(true, true, true);

            //Compress the photo
            if (Photo != null)
                Photo.CompressForVoting();
        }
    }
}
