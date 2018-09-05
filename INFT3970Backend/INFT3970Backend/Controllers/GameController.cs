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
        /// POST: api/player/createGame - Creates a new game and joins a player to their created game, creating a new Player record and returning the generated gamecode created PlayerID
        /// </summary>
        /// <param name="nickname">The players nickname in the game</param>
        /// <param name="contact">The players contact info, either phone or email</param>
        /// <returns>Response including the playerID and gameCode generated in the database. If an error occurs then an empty gamecode and negative PlayerID is returned in the response</returns>
        [HttpPost]
        [Route("api/game/createGame")]
        public ActionResult<Response<KeyValuePair<string, int>>> CreateGame([FromForm] string nickname, [FromForm] string contact) //settings?
        {
            //Example request:
            //Use POSTMAN and POST 'Form-Data' using the following values
            //Key               Value
            //nickname          billy
            //contact           enter an email or phone (NOTE: if using a phone it will send a text message to my number cause the twilio trial can only send to one number)
            //various settings??

            Response<KeyValuePair<string, int>> returnResponse = null;

            GameBL gameBL = new GameBL();
            Response<string> createGameResponse = gameBL.CreateGame();

            //If the create game was successful, join the player to the game as the host player
            if (createGameResponse.Type == ResponseType.SUCCESS)
            {
                PlayerBL playerBL = new PlayerBL();
                Response<int> joinGameResponse = playerBL.JoinGame(createGameResponse.Data, nickname, contact, true);

                //If joining the game was not sucessful we must deactivate the game that was just created
                if (joinGameResponse.Type == ResponseType.ERROR)
                {
                    gameBL.DeactivateGameAfterHostJoinError(createGameResponse.Data);
                    returnResponse = new Response<KeyValuePair<string, int>>(new KeyValuePair<string, int>(string.Empty, -1), ResponseType.ERROR, joinGameResponse.ErrorMessage, joinGameResponse.ErrorCode);
                    return returnResponse;
                }
                //Otherwise, The host successfully joined the game, return the key value pair of gameID and playerID
                else
                {
                    returnResponse = new Response<KeyValuePair<string, int>>(new KeyValuePair<string, int>(createGameResponse.Data, joinGameResponse.Data), ResponseType.SUCCESS, string.Empty, 1);
                    return returnResponse;
                }
            }

            //Otherwise, return the error response when trying to create the game
            else
                return new Response<KeyValuePair<string, int>>(new KeyValuePair<string, int>(string.Empty, -1), ResponseType.ERROR, createGameResponse.ErrorMessage, createGameResponse.ErrorCode);
        }
    }
}
