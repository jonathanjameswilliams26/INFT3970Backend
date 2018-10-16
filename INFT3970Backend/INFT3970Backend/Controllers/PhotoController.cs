using System.Collections.Generic;
using INFT3970Backend.Data_Access_Layer;
using INFT3970Backend.Helpers;
using INFT3970Backend.Hubs;
using INFT3970Backend.Models;
using INFT3970Backend.Models.Errors;
using INFT3970Backend.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace INFT3970Backend.Controllers
{

    [ApiController]
    public class PhotoController : ControllerBase
    {

        //The application hub context, used to be able to invokve client methods from anywhere in the code
        private readonly IHubContext<ApplicationHub> _hubContext;
        public PhotoController(IHubContext<ApplicationHub> hubContext)
        {
            _hubContext = hubContext;
        }


        


        /// <summary>
        /// Uploads a photo to the database. Sends out notifications to players that a photo must now be voted on.
        /// Returns a response which indicates success or error. NULL data is returned.
        /// </summary>
        /// <param name="request">The request which contains all the photo information</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/photo/upload")]
        public ActionResult<Response> Upload(PhotoUploadRequest request)
        {
            try
            {
                var uploadedPhoto = new Photo(request.latitude, request.longitude, request.imgUrl, request.takenByID, request.photoOfID);

                //Call the DAL to save the photo to the DB
                var response = new PhotoDAL().SavePhoto(uploadedPhoto);
                
                //If the response is successful we want to send live updates to clients and
                //email or text message notifications to not connected players
                if (response.IsSuccessful())
                {
                    var hubInterface = new HubInterface(_hubContext);
                    hubInterface.UpdatePhotoUploaded(response.Data);
                    ScheduledTasks.ScheduleCheckPhotoVotingCompleted(response.Data, hubInterface);
                }
                return new Response(response.ErrorMessage, response.ErrorCode);
            }
            //Catch any error associated with invalid model data
            catch (InvalidModelException e)
            {
                return new Response(e.Msg, e.Code);
            }
            //Catch any unhandled / unexpected server errrors
            catch
            {
                return StatusCode(500);
            }
        }





        /// <summary>
        /// Gets the list of PlayerVotePhoto records which have not been completed by the player.
        /// This is the list of photos which the player has not voted on yet.
        /// </summary>
        /// <param name="playerID">The ID of the player</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/photo/vote")]
        public ActionResult<Response<List<Vote>>> GetVotesToComplete([FromHeader] int playerID)
        {
            try
            {
                var player = new Player(playerID);
                //Call the Data Access Layer to get the photos require voting to be completed
                return new PhotoDAL().GetVotesToComplete(player);
            }
            //Catch any error associated with invalid model data
            catch (InvalidModelException e)
            {
                return new Response<List<Vote>>(e.Msg, e.Code);
            }
            //Catch any unhandled / unexpected server errrors
            catch
            {
                return StatusCode(500);
            }
        }







        /// <summary>
        /// Cast a vote on a photo. Decide if the photo is a successful photo or unsuccessful.
        /// </summary>
        /// <param name="playerID">The ID of the player making the vote.</param>
        /// <param name="voteID">The ID of the vote record being updated.</param>
        /// <param name="decision">The decision, TRUE = successful, FALSE = unsuccessful</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/photo/vote")]
        public ActionResult<Response> VoteOnPhoto([FromHeader] int playerID, [FromHeader] int voteID, [FromForm] string decision)
        {
            try
            {
                var vote = new Vote(voteID, decision, playerID);
                var response = new PhotoDAL().VoteOnPhoto(vote);

                if (response.IsSuccessful())
                {
                    //If the Photo's voting has now been completed send the notifications / updates
                    if (response.Data.Photo.IsVotingComplete)
                    {
                        var hubInterface = new HubInterface(_hubContext);
                        hubInterface.UpdatePhotoVotingCompleted(response.Data.Photo);
                    }
                }
                return new Response(response.ErrorMessage, response.ErrorCode);
            }
            //Catch any error associated with invalid model data
            catch (InvalidModelException e)
            {
                return new Response(e.Msg, e.Code);
            }
            //Catch any unhandled / unexpected server errrors
            catch
            {
                return StatusCode(500);
            }
        }
    }
}
