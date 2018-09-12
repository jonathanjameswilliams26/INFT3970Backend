using System;
using System.Collections.Generic;
using INFT3970Backend.Models;
using INFT3970Backend.Data_Access_Layer;
using System.IO;
using SixLabors.ImageSharp;
using Microsoft.AspNetCore.SignalR;
using INFT3970Backend.Hubs;
using Hangfire;
using System.Threading.Tasks;

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

                //Schedule a job to run in order to check to see if the voting has been completed after the voting time limit
                long timeToWait = response.Data.VotingFinishTime.Value.Ticks - DateTime.Now.Ticks;
                BackgroundJob.Schedule(() => ScheduledCheckPhotoVotingCompleted(response.Data, hubInterface), TimeSpan.FromTicks(timeToWait));
            }
            return new Response<object>(null, response.Type, response.ErrorMessage, response.ErrorCode);
            
        }




        



        /// <summary>
        /// This method is a scheduled method which will run after the FinishVotingTime has passed.
        /// The method checks to see if all players have voted on the image and if not, will
        /// update the image to be successful and make all votes a success. Then send out notifications
        /// to the affected players. 
        /// </summary>
        /// <param name="uploadedPhoto">The photo which was uploaded and being checked if voting has been completed.</param>
        /// <param name="hubInterface">The Hub interface which will be used to send notifications / updates</param>
        public static void ScheduledCheckPhotoVotingCompleted(Photo uploadedPhoto, HubInterface hubInterface)
        {
            //Get the updated photo record from the database
            Photo photo = new PhotoDAL().GetPhotoByID(uploadedPhoto.PhotoID);
            if (photo == null)
                return;

            //Confirm the game the photo is apart of is not completed, if completed leave the method
            if (photo.Game.GameState == "COMPLETED")
                return;

            //Check to see if the voting has been completed for the photo.
            //If the voting has been completed exit the method
            if (photo.IsVotingComplete)
                return;

            //Otherwise, the game is not completed and the photo has not been successfully voted on by all players

            //Call the Data Access Layer to update the photo record to now be completed.
            PhotoDAL photoDAL = new PhotoDAL();
            Response<Photo> response = photoDAL.VotingTimeExpired(photo.PhotoID);

            //If the update was successful then send out the notifications to the affected players
            //Will send out in game notifications and text/email notifications
            if(response.Type == "SUCCESS")
                hubInterface.UpdatePhotoVotingCompleted(response.Data);
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
    }
}
