using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using INFT3970Backend.Data_Access_Layer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using INFT3970Backend.Models;
using INFT3970Backend.Business_Logic_Layer;
using Microsoft.AspNetCore.SignalR;
using INFT3970Backend.Hubs;

namespace INFT3970Backend.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class PlayerController : ControllerBase
    {
        //The application hub context, used to be able to invokve client methods from anywhere in the code
        private readonly IHubContext<ApplicationHub> _hubContext;
        public PlayerController(IHubContext<ApplicationHub> hubContext)
        {
            _hubContext = hubContext;
        }


        /// <summary>
        /// GET: api/player/getAllPlayersInGame/{playerID} - 
        /// Gets a list of all the players inside a game. Takes in a playerID and uses that ID to find the GameID and then get all players inside that Game
        /// </summary>
        /// <param name="playerID">The playerID used to determine which game the player is in and get all players in that game</param>
        /// <returns>A list of players inside the same GameID as the PlayerID passed in</returns>
        [HttpGet]
        [Route("api/player/getAllPlayersInGame/{playerID:int}")]
        public ActionResult<Response<List<Player>>> GetAllPlayersInGame(int playerID)
        {
            //Example request
            //https://localhost:5000/api/player/getAllPlayersInGame/100000

            PlayerBL playerBL = new PlayerBL();
            return playerBL.GetAllPlayersInGame(playerID);
        }






        /// <summary>
        /// POST: api/Player/SetConnectionID - Sets the connection ID of the playerID passed in
        /// </summary>
        /// <param name="PlayerIDAndConnectionID">Key Value pair of JSON body data representing the PlayerID and their connectionID to the hub</param>
        /// <returns>Response with NULL data, outlining if the database update was SUCCESSFUL or ERROR</returns>
        [HttpPost]
        [Route("api/player/setConnectionID")]
        public ActionResult<Response<object>> SetConnectionID([FromBody] KeyValuePair<int, string> PlayerIDAndConnectionID)
        {
            // Example Request (JSON)
            /*
             *  {
                    "key": 1234,
                    "value": "connectionIDValue"
                }
             */
            PlayerBL playerBL = new PlayerBL();
            return playerBL.UpdateConnectionID(PlayerIDAndConnectionID.Key, PlayerIDAndConnectionID.Value);
        }





        /// <summary>
        /// POST: api/player/joinGame - Joins a player to a game matching the gameCode value, 
        /// creating a new Player record and returning the created Player object.
        /// </summary>
        /// <param name="gameCode">The gamecode the player is trying to join</param>
        /// <param name="nickname">The players nickname in the game</param>
        /// <param name="contact">The players contact info, either phone or email</param>
        /// <returns>Response including the created Player object. NULL data if an error occurred.</returns>
        [HttpPost]
        [Route("api/player/joinGame")]
        public ActionResult<Response<Player>> JoinGame([FromForm] string gameCode, [FromForm] string nickname, [FromForm] string contact)
        {
            //Example request:
            //Use POSTMAN and POST 'Form-Data' using the following values
            //Key               Value
            //gameCode          tcf124
            //nickname          billy
            //contact           enter an email or phone (NOTE: if using a phone it will send a text message to my number cause the twilio trial can only send to one number)

            //Call the business logic layer to validate the form data and create a new player
            return new PlayerBL().JoinGame(gameCode, nickname, contact, false);
        }







        /// <summary>
        /// Verifies a player by checking the verification code they have entered is correct. 
        /// Returns a Response indicating success or failure.
        /// </summary>
        /// <param name="verificationCode">The verification code received by the user via phone or email.</param>
        /// <returns>Response with NULL data, outlining if the database update was SUCCESSFUL or ERROR</returns>
        [HttpPost]
        [Route("api/player/verify")]
        public Response<object> VerifyPlayer([FromForm] string verificationCode, [FromHeader] int playerID)
        {
            //Call the business logic to verify the player and return a SUCCESS or ERROR response
            return new PlayerBL().VerifyPlayer(verificationCode, playerID, _hubContext);
        }






        /// <summary>
        /// Generates a new verification code for the player, updates the verification code in the database
        /// and resends the new code to the player's contact information (Email or Phone).
        /// </summary>
        /// <param name="playerID">The playerID who's verification code is being updated</param>
        /// <returns>Response with NULL data, outlining if the database update was SUCCESSFUL or ERROR</returns>
        [HttpPost]
        [Route("api/player/resend")]
        public Response<object> ResendVerificationCode([FromHeader] int playerID)
        {
            //Call the business logic layer
            return new PlayerBL().ResendVerificationCode(playerID);
        }
    }
}
