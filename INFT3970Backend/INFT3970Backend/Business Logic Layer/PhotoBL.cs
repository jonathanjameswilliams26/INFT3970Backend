using System;
using System.Collections.Generic;
using INFT3970Backend.Models;
using INFT3970Backend.Data_Access_Layer;
using Microsoft.AspNetCore.SignalR;
using INFT3970Backend.Hubs;

namespace INFT3970Backend.Business_Logic_Layer
{
    public class PhotoBL
    {

        public Response<List<Photo>> GetPhotoLocation(int photoID)
        {
            PhotoDAL photoDAL = new PhotoDAL();
            return photoDAL.GetPhotoLocation(photoID);
        }





        /// <summary>
        /// Saves a photo to the database and sends out notifications to players that a new photo has been uploaded and ready to vote on.
        /// Returns a response will NULL data to indicate success or fail.
        /// </summary>
        /// <param name="imgUrl">The base64 DataURL of the image to save.</param>
        /// <param name="tempTakenByID">A string of the PlayerID who took the photo</param>
        /// <param name="tempPhotoOfID">A string of the playerID who the photo is of</param>
        /// <param name="hubContext">The Application Hub context used to provide live updates via SignalR</param>
        /// <param name="tempLatitude">A string of the latitude value</param>
        /// <param name="tempLongitude">A string of the longitude value</param>
        /// <returns></returns>
        public Response<object> SavePhoto(string imgUrl, string tempTakenByID, string tempPhotoOfID, IHubContext<ApplicationHub> hubContext, string tempLatitude, string tempLongitude)
        {
            //Confirm the data is in the correct format
            int takenByID = 0;
            int photoOfID = 0;
            double latitude = 0;
            double longitude = 0;
            try
            {
                takenByID = int.Parse(tempTakenByID);
                photoOfID = int.Parse(tempPhotoOfID);
                latitude = double.Parse(tempLatitude);
                longitude = double.Parse(tempLongitude);
                var base64Data = imgUrl.Replace("data:image/jpeg;base64,", "");
                var binData = Convert.FromBase64String(base64Data);
            }
            catch
            {
                return new Response<object>(null, "ERROR", "The data provided is invalid, check the takenByID, PhotoOfID, lat, long and DataURL values to ensure they are in the correct format.", ErrorCodes.EC_DATAINVALID);
            }

            //Save the DataURL to the database
            Photo photo = new Photo
            {
                PhotoDataURL = imgUrl,
                TakenByPlayerID = takenByID,
                PhotoOfPlayerID = photoOfID,
                Lat = latitude,
                Long = longitude
            };
            PhotoDAL photoDAL = new PhotoDAL();
            Response<Photo> response = photoDAL.SavePhoto(photo);

            //If the response is successful we want to send live updates to clients and
            //email or text message notifications to not connected players
            if(response.Type == "SUCCESS")
            {
                HubInterface hubInterface = new HubInterface(hubContext);
                hubInterface.UpdatePhotoUploaded(response.Data);
                ScheduledTasks.ScheduleCheckPhotoVotingCompleted(response.Data, hubInterface);
            }
            return new Response<object>(null, response.Type, response.ErrorMessage, response.ErrorCode);
            
        }






        /// <summary>
        /// Gets the list of all PlayerVotePhoto records which have not been completed by the specified playerID
        /// </summary>
        /// <param name="playerID">The PlayerID who's incomplete voting records will be returned</param>
        /// <returns></returns>
        public Response<List<PlayerVotePhoto>> GetVotesToComplete(int playerID)
        {
            //Call the Data Access Layer to get the photos require voting to be completed
            PhotoDAL photoDAL = new PhotoDAL();
            return photoDAL.GetVotesToComplete(playerID);
        }





        /// <summary>
        /// Cast a vote on a photo and decide if the photo is successful or unsuccessful
        /// </summary>
        /// <param name="playerID">The playerID who is casting the vote</param>
        /// <param name="voteID">The ID of the vote record</param>
        /// <param name="decision">The decision, true = successful, false = unsuccessful</param>
        /// <param name="hubContext">The hub context used to send out live updates to players.</param>
        /// <returns></returns>
        public Response<object> VoteOnPhoto(int playerID, int voteID, string decision, IHubContext<ApplicationHub> hubContext)
        {
            //Confirm the decision is in the correct format
            bool isPhotoSuccessful = false;
            try
            {
                isPhotoSuccessful = bool.Parse(decision);
            }
            catch
            {
                return new Response<object>(null, "ERROR", "The decision data is invalid. Should only be TRUE or FALSE.", ErrorCodes.EC_DATAINVALID);
            }

            //Update the Vote record in the database
            PlayerVotePhoto playerVotePhoto = new PlayerVotePhoto
            {
                PlayerID = playerID,
                VoteID = voteID,
                IsPhotoSuccessful = isPhotoSuccessful
            };
            PhotoDAL photoDAL = new PhotoDAL();
            Response<PlayerVotePhoto> response = photoDAL.VoteOnPhoto(playerVotePhoto);


            if(response.Type == "SUCCESS")
            {
                //If the Photo's voting has now been completed send the notifications / updates
                if(response.Data.Photo.IsVotingComplete)
                {
                    HubInterface hubInterface = new HubInterface(hubContext);
                    hubInterface.UpdatePhotoVotingCompleted(response.Data.Photo);
                }
            }

            return new Response<object>(null, response.Type, response.ErrorMessage, response.ErrorCode);
        }





        public Response<List<Photo>> GetLastKnownLocations(int playerID)
        {
            //Call the data access layer to get the last known locations
            PhotoDAL photoDAL = new PhotoDAL();
            return photoDAL.GetLastKnownLocations(playerID);
        }
    }
}
