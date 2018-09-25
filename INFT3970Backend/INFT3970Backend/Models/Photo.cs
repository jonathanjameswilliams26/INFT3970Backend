using INFT3970Backend.Models.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Backend.Models
{
    public class Photo
    {
        private int photoID;
        private double lat;
        private double longitude;
        private string photoDataURL;
        private DateTime? timeTaken;
        private DateTime? votingFinishTime;
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
                var errorMessage =  ErrorMessages.EM_PHOTO_MODELINVALID 
                                    + "PhotoID must be atleast 100000.";

                if (value == -1 || value >= 100000)
                    photoID = value;

                else
                    throw new InvalidModelException(errorMessage, ErrorCodes.EC_PLAYER_ID);
            }
        }



        public double Lat
        {
            get { return lat; }
            set
            {
                var errorMessage = ErrorMessages.EM_PHOTO_MODELINVALID
                                    + "Latitude is not within the valid range. Must be -90 to +90";

                if (value >= -90 || value <= 90)
                    lat = value;

                else
                    throw new InvalidModelException(errorMessage, ErrorCodes.EC_PLAYER_ID);
            }
        }



        public double Long
        {
            get { return longitude; }
            set
            {
                var errorMessage = ErrorMessages.EM_PHOTO_MODELINVALID
                                    + "Latitude is not within the valid range. Must be -180 to +180";

                if (value >= -180 || value <= 180)
                    longitude = value;

                else
                    throw new InvalidModelException(errorMessage, ErrorCodes.EC_PLAYER_ID);
            }
        }



        public string PhotoDataURL
        {
            get { return photoDataURL; }
            set
            {
                var errorMessage = ErrorMessages.EM_PHOTO_MODELINVALID
                                    + "PhotoDataURL is not a base64 string.";

                if (value == "empty")
                {
                    photoDataURL = value;
                    return;
                }

                if (string.IsNullOrWhiteSpace(value))
                    throw new InvalidModelException(errorMessage, ErrorCodes.EC_PLAYER_SELFIE);

                //Confirm the imgURL is a base64 string
                try
                {
                    if (!value.Contains("data:image/jpeg;base64,"))
                        throw new InvalidModelException(errorMessage, ErrorCodes.EC_PLAYER_SELFIE);
                    var base64Data = value.Replace("data:image/jpeg;base64,", "");
                    var byteData = Convert.FromBase64String(base64Data);
                    photoDataURL = value;
                }
                catch
                {
                    throw new InvalidModelException(ErrorMessages.EM_PLAYER_SELFIE, ErrorCodes.EC_PLAYER_SELFIE);
                }
            }
        }



        public DateTime? TimeTaken
        {
            get { return timeTaken; }
            set
            {
                var errorMessage = ErrorMessages.EM_PHOTO_MODELINVALID
                                    + "TimeTaken is greater than VotingFinishTime.";

                if (value == null)
                    timeTaken = null;

                else if (VotingFinishTime.Value == null)
                    timeTaken = value;

                //If an voting finish time exists, confirm the time taken is less than
                else
                {
                    if (value.Value > VotingFinishTime.Value)
                        throw new InvalidModelException(errorMessage, ErrorCodes.EC_GAME_DATES);
                    else
                        timeTaken = value;
                }
            }
        }



        public DateTime? VotingFinishTime
        {
            get { return votingFinishTime; }
            set
            {
                var errorMessage = ErrorMessages.EM_PHOTO_MODELINVALID
                                    + "VotingFinishTime is less than TimeTaken.";

                if (value == null)
                    votingFinishTime = null;

                else if (TimeTaken.Value == null)
                    votingFinishTime = value;

                //If a time taken exists, confirm the voting finish time is greater
                else
                {
                    if (TimeTaken.Value > value.Value)
                        throw new InvalidModelException(errorMessage, ErrorCodes.EC_GAME_DATES);
                    else
                        votingFinishTime = value;
                }
            }
        }



        public int NumYesVotes
        {
            get { return numYesVotes; }
            set
            {
                var errorMessage = ErrorMessages.EM_PHOTO_MODELINVALID
                                    + "Number of yes votes cannot be below 0.";

                if (value < 0)
                    throw new InvalidModelException(errorMessage, ErrorCodes.EC_PLAYER_AMMO);
                else
                    numYesVotes = value;
            }
        }



        public int NumNoVotes
        {
            get { return numNoVotes; }
            set
            {
                var errorMessage = ErrorMessages.EM_PHOTO_MODELINVALID
                                    + "Number of no votes cannot be below 0.";

                if (value < 0)
                    throw new InvalidModelException(errorMessage, ErrorCodes.EC_PLAYER_AMMO);
                else
                    numNoVotes = value;
            }
        }
        public bool IsVotingComplete { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }



        public int GameID
        {
            get { return gameID; }
            set
            {
                var errorMessage = ErrorMessages.EM_PHOTO_MODELINVALID
                                    + "GameID must be atleast 100000.";

                if (value == -1 || value >= 100000)
                    gameID = value;

                else
                    throw new InvalidModelException(errorMessage, ErrorCodes.EC_PLAYER_ID);
            }
        }



        public int TakenByPlayerID
        {
            get { return takenByPlayerID; }
            set
            {
                var errorMessage = ErrorMessages.EM_PHOTO_MODELINVALID
                                    + "TakenByPlayerID must be atleast 100000.";

                if (value == -1 || value >= 100000)
                    takenByPlayerID = value;

                else
                    throw new InvalidModelException(errorMessage, ErrorCodes.EC_PLAYER_ID);
            }
        }



        public int PhotoOfPlayerID
        {
            get { return photoOfPlayerID; }
            set
            {
                var errorMessage = ErrorMessages.EM_PHOTO_MODELINVALID
                                    + "photoOfPlayerID must be atleast 100000.";

                if (value == -1 || value >= 100000)
                    photoOfPlayerID = value;

                else
                    throw new InvalidModelException(errorMessage, ErrorCodes.EC_PLAYER_ID);
            }
        }



        public Game Game { get; set; }
        public Player TakenByPlayer { get; set; }
        public Player PhotoOfPlayer { get; set; }


        public Photo()
        {
            PhotoID = -1;
            GameID = -1;
            TakenByPlayerID = -1;
            PhotoOfPlayerID = -1;
            PhotoDataURL = "empty";
        }

        public Photo(double lat, double longitude, string photoDataURL, int takenByPlayerID, int photoOfPlayerID)
        {
            PhotoID = -1;
            GameID = -1;
            Lat = lat;
            Long = longitude;
            PhotoDataURL = photoDataURL;
            TakenByPlayerID = takenByPlayerID;
            PhotoOfPlayerID = photoOfPlayerID;
        }

        public Photo(string lat, string longitude, string photoDataURL, string takenByPlayerID, string photoOfPlayerID)
        {
            PhotoID = -1;
            GameID = -1;
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
                var errorMessage = ErrorMessages.EM_PHOTO_MODELINVALID
                                    + "Lat, long, takenByPlayerID or photoOfID is invalid format. Must be numbers.";
                throw new InvalidModelException(errorMessage, 0);
            }
        }
    }
}
