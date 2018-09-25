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
        public Response SavePhoto(Photo photo, IHubContext<ApplicationHub> hubContext)
        {
            Response<Photo> response = new PhotoDAL().SavePhoto(photo);

            //If the response is successful we want to send live updates to clients and
            //email or text message notifications to not connected players
            if(response.IsSuccessful())
            {
                HubInterface hubInterface = new HubInterface(hubContext);
                hubInterface.UpdatePhotoUploaded(response.Data);
                ScheduledTasks.ScheduleCheckPhotoVotingCompleted(response.Data, hubInterface);
            }
            return new Response(response.ErrorMessage, response.ErrorCode);
        }






        /// <summary>
        /// Gets the list of all PlayerVotePhoto records which have not been completed by the specified playerID
        /// </summary>
        /// <param name="playerID">The PlayerID who's incomplete voting records will be returned</param>
        /// <returns></returns>
        public Response<List<Vote>> GetVotesToComplete(Player player)
        {
            //Call the Data Access Layer to get the photos require voting to be completed
            return new PhotoDAL().GetVotesToComplete(player);
        }





        /// <summary>
        /// Cast a vote on a photo and decide if the photo is successful or unsuccessful
        /// </summary>
        /// <param name="playerID">The playerID who is casting the vote</param>
        /// <param name="voteID">The ID of the vote record</param>
        /// <param name="decision">The decision, true = successful, false = unsuccessful</param>
        /// <param name="hubContext">The hub context used to send out live updates to players.</param>
        /// <returns></returns>
        public Response VoteOnPhoto(Vote vote, IHubContext<ApplicationHub> hubContext)
        {
            PhotoDAL photoDAL = new PhotoDAL();
            Response<Vote> response = photoDAL.VoteOnPhoto(vote);

            if(response.IsSuccessful())
            {
                //If the Photo's voting has now been completed send the notifications / updates
                if(response.Data.Photo.IsVotingComplete)
                {
                    HubInterface hubInterface = new HubInterface(hubContext);
                    hubInterface.UpdatePhotoVotingCompleted(response.Data.Photo);
                }
            }

            return new Response(response.ErrorMessage, response.ErrorCode);
        }





        public Response<List<Photo>> GetLastKnownLocations(Player player)
        {
            //Call the data access layer to get the last known locations
            return new PhotoDAL().GetLastKnownLocations(player);
        }
    }
}
