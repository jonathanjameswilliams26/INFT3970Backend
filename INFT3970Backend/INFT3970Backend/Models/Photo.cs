using INFT3970Backend.Models.Errors;
using System;

namespace INFT3970Backend.Models
{
    public class Photo
    {
        //Private backing stores of public properites which have business logic behind them.
        private int photoID;
        private double lat;
        private double longitude;
        private string photoDataURL;
        private int numYesVotes;
        private int numNoVotes;
        private int gameID;
        private int takenByPlayerID;
        private int photoOfPlayerID;


        public int PhotoID
        {
            get { return photoID; }
            set
            {
                var errorMessage =  "PhotoID must be atleast 100000.";

                if (value == -1 || value >= 100000)
                    photoID = value;

                else
                    throw new InvalidModelException(errorMessage, ErrorCodes.MODELINVALID_PHOTO);
            }
        }



        public double Lat
        {
            get { return lat; }
            set
            {
                var errorMessage = "Latitude is not within the valid range. Must be -90 to +90";

                if (value >= -90 && value <= 90)
                    lat = value;

                else
                    throw new InvalidModelException(errorMessage, ErrorCodes.MODELINVALID_PHOTO);
            }
        }



        public double Long
        {
            get { return longitude; }
            set
            {
                var errorMessage = "Longitude is not within the valid range. Must be -180 to +180";

                if (value >= -180 && value <= 180)
                    longitude = value;

                else
                    throw new InvalidModelException(errorMessage, ErrorCodes.MODELINVALID_PHOTO);
            }
        }



        public string PhotoDataURL
        {
            get { return photoDataURL; }
            set
            {
                var errorMessage = "PhotoDataURL is not a base64 string.";

                if (value == null)
                {
                    photoDataURL = value;
                    return;
                }

                if (string.IsNullOrWhiteSpace(value))
                    throw new InvalidModelException(errorMessage, ErrorCodes.MODELINVALID_PHOTO);

                //Confirm the imgURL is a base64 string
                try
                {
                    if (!value.Contains("data:image/jpeg;base64,"))
                        throw new InvalidModelException(errorMessage, ErrorCodes.MODELINVALID_PHOTO);

                    var base64Data = value.Replace("data:image/jpeg;base64,", "");
                    var byteData = Convert.FromBase64String(base64Data);
                    photoDataURL = value;
                }
                catch
                {
                    throw new InvalidModelException(errorMessage, ErrorCodes.MODELINVALID_PHOTO);
                }
            }
        }



        public int NumYesVotes
        {
            get { return numYesVotes; }
            set
            {
                var errorMessage = "Number of yes votes cannot be below 0.";

                if (value < 0)
                    throw new InvalidModelException(errorMessage, ErrorCodes.MODELINVALID_PHOTO);
                else
                    numYesVotes = value;
            }
        }



        public int NumNoVotes
        {
            get { return numNoVotes; }
            set
            {
                var errorMessage = "Number of no votes cannot be below 0.";

                if (value < 0)
                    throw new InvalidModelException(errorMessage, ErrorCodes.MODELINVALID_PHOTO);
                else
                    numNoVotes = value;
            }
        }
        



        public int GameID
        {
            get { return gameID; }
            set
            {
                var errorMessage = "GameID must be atleast 100000.";

                if (value == -1 || value >= 100000)
                    gameID = value;

                else
                    throw new InvalidModelException(errorMessage, ErrorCodes.MODELINVALID_PHOTO);
            }
        }



        public int TakenByPlayerID
        {
            get { return takenByPlayerID; }
            set
            {
                var errorMessage = "TakenByPlayerID must be atleast 100000.";

                if (value == -1 || value >= 100000)
                    takenByPlayerID = value;

                else
                    throw new InvalidModelException(errorMessage, ErrorCodes.MODELINVALID_PHOTO);
            }
        }



        public int PhotoOfPlayerID
        {
            get { return photoOfPlayerID; }
            set
            {
                var errorMessage = "photoOfPlayerID must be atleast 100000.";

                if (value == -1 || value >= 100000)
                    photoOfPlayerID = value;

                else
                    throw new InvalidModelException(errorMessage, ErrorCodes.MODELINVALID_PHOTO);
            }
        }



        public bool IsSuccessful
        {
            get
            {
                if (!IsVotingComplete)
                    return false;
                else
                    return NumYesVotes > NumNoVotes;
            }
        }



        public DateTime TimeTaken { get; set; }
        public DateTime VotingFinishTime { get; set; }
        public bool IsVotingComplete { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public Game Game { get; set; }
        public Player TakenByPlayer { get; set; }
        public Player PhotoOfPlayer { get; set; }
        


        /// <summary>
        /// Creates a Photo with default values
        /// </summary>
        public Photo()
        {
            PhotoID = -1;
            GameID = -1;
            TakenByPlayerID = -1;
            PhotoOfPlayerID = -1;
            PhotoDataURL = null;
            TimeTaken = DateTime.Now;
            VotingFinishTime = DateTime.Now.AddMinutes(15);
            IsActive = true;
        }



        /// <summary>
        /// Creates a photo with default values and sets the following properties.
        /// </summary>
        /// <param name="lat">Latitude</param>
        /// <param name="longitude">Longitude</param>
        /// <param name="photoDataURL">The DataURL of the photo, base64 string</param>
        /// <param name="takenByPlayerID">The ID of the player who took the photo</param>
        /// <param name="photoOfPlayerID">The ID of the player who the photo is of</param>
        public Photo(double lat, double longitude, string photoDataURL, int takenByPlayerID, int photoOfPlayerID) : this()
        {
            Lat = lat;
            Long = longitude;
            PhotoDataURL = photoDataURL;
            TakenByPlayerID = takenByPlayerID;
            PhotoOfPlayerID = photoOfPlayerID;
        }



        /// <summary>
        /// Creates a photo with default values and sets the following properties.
        /// </summary>
        /// <param name="lat">Latitude</param>
        /// <param name="longitude">Longitude</param>
        /// <param name="photoDataURL">The DataURL of the photo, base64 string</param>
        /// <param name="takenByPlayerID">The ID of the player who took the photo</param>
        /// <param name="photoOfPlayerID">The ID of the player who the photo is of</param>
        public Photo(string lat, string longitude, string photoDataURL, string takenByPlayerID, string photoOfPlayerID) :this()
        {
            PhotoDataURL = photoDataURL;

            try
            {
                Lat = double.Parse(lat);
                Long = double.Parse(longitude);
                TakenByPlayerID = int.Parse(takenByPlayerID);
                PhotoOfPlayerID = int.Parse(photoOfPlayerID);
            }
            catch(FormatException)
            {
                var errorMessage = "Lat, long, takenByPlayerID or photoOfID is invalid format. Must be numbers.";
                throw new InvalidModelException(errorMessage, ErrorCodes.MODELINVALID_PHOTO);
            }
        }


        public void CompressForMapRequest()
        {
            PhotoDataURL = null;

            //Compress the player object to remove all the photos except for the extraSmallPhoto
            if (TakenByPlayer != null)
                TakenByPlayer.Compress(true, true, false);

            if (PhotoOfPlayer != null)
                PhotoOfPlayer.Compress(true, true, true);
        }


        public void CompressForVoting()
        {
            //Compress the taken by player as its not needed for voting
            if (TakenByPlayer != null)
                TakenByPlayer.Compress(true, true, true);

            //Compress the photo of player but keep their large selfie
            if (PhotoOfPlayer != null)
                PhotoOfPlayer.Compress(false, true, true);
        }
    }
}
