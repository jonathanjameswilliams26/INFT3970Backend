using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using INFT3970Backend.Business_Logic_Layer;
using INFT3970Backend.Hubs;
using INFT3970Backend.Models;
using Microsoft.AspNetCore.Http;
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
        /// <param name="imgUrl">The base64 dataURL of the image captured and to be saved in the DB</param>
        /// <param name="takenByID">The ID of the player who took the photo</param>
        /// <param name="photoOfID">The ID of the player who the photo is of.</param>
        /// <param name="latitude">The latitude the photo was captured at</param>
        /// <param name="longitude">The longitude the photo was captured at.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/photo/upload")]
        public ActionResult<Response<object>> Upload([FromForm] string imgUrl, [FromForm] string takenByID, [FromForm] string photoOfID, [FromForm] string latitude, [FromForm] string longitude)
        {
            PhotoBL photoBL = new PhotoBL();
            return photoBL.SavePhoto(imgUrl, takenByID, photoOfID, _hubContext, latitude, longitude);
        }





        /// <summary>
        /// Gets the list of PlayerVotePhoto records which have not been completed by the player.
        /// This is the list of photos which the player has not voted on yet.
        /// </summary>
        /// <param name="playerID">The ID of the player</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/photo/vote")]
        public ActionResult<Response<List<PlayerVotePhoto>>> GetVotesToComplete([FromHeader] int playerID)
        {
            //Call the business logic layer to get the list of votes the player needs to complete
            PhotoBL photoBL = new PhotoBL();
            return photoBL.GetVotesToComplete(playerID);
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
        public ActionResult<Response<object>> VoteOnPhoto([FromHeader] int playerID, [FromHeader] int voteID, [FromForm] string decision)
        {
            //Call the business logic layer to cast the vote on the photo
            PhotoBL photoBL = new PhotoBL();
            return photoBL.VoteOnPhoto(playerID, voteID, decision, _hubContext);
        }




        [HttpGet]
        [Route("api/photo/lastKnownLocations")]
        public ActionResult<Response<List<Photo>>> GetLastKnownLocations([FromHeader] int playerID)
        {
            //Call the business logic layer to cast the vote on the photo
            PhotoBL photoBL = new PhotoBL();
            return photoBL.GetLastKnownLocations(playerID);
        }

    }
}
