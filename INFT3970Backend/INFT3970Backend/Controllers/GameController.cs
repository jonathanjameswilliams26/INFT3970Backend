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
        //The application hub context, used to be able to invokve client methods from anywhere in the code
        private readonly IHubContext<ApplicationHub> _hubContext;
        public GameController(IHubContext<ApplicationHub> hubContext)
        {
            _hubContext = hubContext;
        }


        /// <summary>
        /// POST: api/player/createGame - Creates a new game and joins a player to their created game, 
        /// creating a new Player record and returning the created Player object with the created Game data/object also
        /// </summary>
        /// <param name="nickname">The players nickname in the game</param>
        /// <param name="contact">The players contact info, either phone or email</param>
        /// <param name="imgUrl">The imgUrl of the players profile picture</param>
        /// <returns>Response including the created Player object, including Game data. NULL data if error occurred.</returns>
        [HttpPost]
        [Route("api/game/createGame")]
        public ActionResult<Response<Player>> CreateGame([FromForm] string nickname, [FromForm] string contact, [FromForm] string imgUrl) //settings?
        {
            //Example request:
            //Use POSTMAN and POST 'Form-Data' using the following values
            //Key               Value
            //nickname          billy
            //contact           enter an email or phone (NOTE: if using a phone it will send a text message to my number cause the twilio trial can only send to one number)
            //imgUrl            the imgUrl string of the players profile picture
            //various settings??

            Response<Player> returnResponse = null;

            GameBL gameBL = new GameBL();
            Response<Game> createGameResponse = gameBL.CreateGame();

            //If the create game was successful, join the player to the game as the host player
            if (createGameResponse.Type == "SUCCESS")
            {
                PlayerBL playerBL = new PlayerBL();
                Response<Player> joinGameResponse = playerBL.JoinGame(createGameResponse.Data.GameCode, nickname, contact, imgUrl, true);

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

        [HttpGet]
        [Route("api/game/getGame/{gameID:int}")]
        public ActionResult<Response<Game>> GetGame(int gameID)
        {
            //Example request
            //https://localhost:5000/api/game/getGame/100000

      
            GameBL gameBL = new GameBL();
            return gameBL.GetGame(gameID);
        }





        /// <summary>
        /// Test complete game API endpoint
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/game/complete/{gameID:int}")]
        public ActionResult<Response<object>> CompleteGame(int gameID)
        {
            //Example request
            //https://localhost:5000/api/game/complete/100000


            GameBL gameBL = new GameBL();
            return gameBL.CompleteGame(gameID, _hubContext);
        }
    }
}
