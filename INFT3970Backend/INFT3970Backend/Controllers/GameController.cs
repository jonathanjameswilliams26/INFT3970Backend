using INFT3970Backend.Business_Logic_Layer;
using INFT3970Backend.Hubs;
using INFT3970Backend.Models;
using INFT3970Backend.Models.Requests;
using INFT3970Backend.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;

namespace INFT3970Backend.Controllers
{
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
        public ActionResult<Response<Player>> CreateGame(CreateGameRequest request) //settings?
        {
            //Example request:
            //Use POSTMAN and POST 'Form-Data' using the following values
            //Key               Value
            //nickname          billy
            //contact           enter an email or phone (NOTE: if using a phone it will send a text message to my number cause the twilio trial can only send to one number)
            //imgUrl            the imgUrl string of the players profile picture
            //various settings??

            GameBL gameBL = new GameBL();
            return gameBL.CreateGame(request.nickname, request.contact, request.imgUrl);
        }








        /// <summary>
        /// Gets the Game information matching the specified ID
        /// </summary>
        /// <param name="gameID">The GameID</param>
        /// <returns>A Game object, NULL if error occurred</returns>
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
        /// Gets all the players in a game with multiple filter parameters
        /// 
        /// FILTER
        /// ALL = get all the players in the game which arnt deleted
        /// ACTIVE = get all players in the game which arnt deleted and is active
        /// INGAME = get all players in the game which arnt deleted, is active, have not left the game and have been verified
        /// INGAMEALL = get all players in the game which arnt deleted, is active, and have been verified(includes players who have left the game)
        ///
        /// ORDER by
        /// AZ = Order by name in alphabetical order
        /// ZA = Order by name in reverse alphabetical order
        /// KILLS= Order from highest to lowest in number of kills
        /// </summary>
        /// <param name="id">The playerID or the GameID</param>
        /// <param name="isPlayerID">A flag which outlines if the ID passed in is a playerID</param>
        /// <param name="filter">The filter value, ALL, ACTIVE, INGAME, INGAMEALL</param>
        /// <param name="orderBy">The order by value, AZ, ZA, KILLS</param>
        /// <returns>The list of all players in the game</returns>
        [HttpGet]
        [Route("api/game/getAllPlayersInGame/{id:int}/{isPlayerID:bool}/{filter}/{orderBy}")]
        public ActionResult<Response<Game>> GetAllPlayersInGame(int id, bool isPlayerID, string filter, string orderBy)
        {
            //Example request
            //https://localhost:5000/api/game/getAllPlayersInGame/100000/true/INGAME/AZ


            GameBL gameBL = new GameBL();
            return gameBL.GetAllPlayersInGame(id, isPlayerID, filter, orderBy);
        }








        /// <summary>
        /// Begins the Game
        /// </summary>
        /// <param name="playerID">The ID of the host player, the host player is the only player who can begin the game</param>
        /// <returns>The updated Game object after being updated in the database.</returns>
        [HttpPost]
        [Route("api/game/begin")]
        public ActionResult<Response<Game>> BeginGame([FromHeader] int playerID)
        {
            //Example request
            //https://localhost:5000/api/game/begin

            GameBL gameBL = new GameBL();
            return gameBL.BeginGame(playerID, _hubContext);
        }






        /// <summary>
        /// Get the current status of the game / web application. Used when a user reconnects
        /// back to the web application in order to the front end to be updated with the current
        /// game / application state so the front end can redirect the user accordingly.
        /// </summary>
        /// <param name="playerID">The ID of the player</param>
        /// <returns>
        /// A GameStatusResponse object which outlines the GameState, 
        /// if the player has votes to complete, if the player has any new notifications 
        /// and the most recent player record. NULL if an error occurred.
        /// </returns>
        [HttpGet]
        [Route("api/game/status")]
        public ActionResult<Response<GameStatusResponse>> GetGameStatus([FromHeader] int playerID)
        {
            //Example request
            //https://localhost:5000/api/game/status

            GameBL gameBL = new GameBL();
            return gameBL.GetGameStatus(playerID);
        }
    }
}
