using INFT3970Backend.Business_Logic_Layer;
using INFT3970Backend.Hubs;
using INFT3970Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;

namespace INFT3970Backend.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {

        /// <summary>
        /// POST: api/player/createGame - Creates a new game and joins a player to their created game, 
        /// creating a new Player record and returning the created Player object with the created Game data/object also
        /// </summary>
        /// <param name="nickname">The players nickname in the game</param>
        /// <param name="contact">The players contact info, either phone or email</param>
        /// <returns>Response including the created Player object, including Game data. NULL data if error occurred.</returns>
        [HttpPost]
        [Route("api/game/createGame")]
        public ActionResult<Response<Player>> CreateGame([FromForm] string nickname, [FromForm] string contact) //settings?
        {
            //Example request:
            //Use POSTMAN and POST 'Form-Data' using the following values
            //Key               Value
            //nickname          billy
            //contact           enter an email or phone (NOTE: if using a phone it will send a text message to my number cause the twilio trial can only send to one number)
            //various settings??

            Response<Player> returnResponse = null;

            GameBL gameBL = new GameBL();
            Response<Game> createGameResponse = gameBL.CreateGame();

            //If the create game was successful, join the player to the game as the host player
            if (createGameResponse.Type == "SUCCESS")
            {
                PlayerBL playerBL = new PlayerBL();
                Response<Player> joinGameResponse = playerBL.JoinGame(createGameResponse.Data.GameCode, nickname, contact, true);

                //If joining the game was not sucessful we must deactivate the game that was just created
                if (joinGameResponse.Type == "ERROR")
                {
                    gameBL.DeactivateGameAfterHostJoinError(createGameResponse.Data.GameID);
                    returnResponse = new Response<Player>(null, "ERROR", joinGameResponse.ErrorMessage, joinGameResponse.ErrorCode);
                    return returnResponse;
                }
                //Otherwise, The host successfully joined the game, return the Player object
                else
                    return joinGameResponse;
            }

            //Otherwise, return the error response when trying to create the game
            else
                return new Response<Player>(null, "ERROR", createGameResponse.ErrorMessage, createGameResponse.ErrorCode);
        }
    }
}
