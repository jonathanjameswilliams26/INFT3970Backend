using INFT3970Backend.Hubs;
using INFT3970Backend.Models;
using INFT3970Backend.Models.Requests;
using INFT3970Backend.Models.Responses;
using INFT3970Backend.Models.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using INFT3970Backend.Data_Access_Layer;
using INFT3970Backend.Helpers;

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
        /// creating a new Player record and returning the created Player object with the created Game data/object.
        /// </summary>
        /// <param name="request">The request object containing the player information.</param>
        /// <returns>The created Player record.</returns>
        [HttpPost]
        [Route("api/game/createGame")]
        public ActionResult<Response<Player>> CreateGame(CreateGameRequest request)
        {
            //Create the player object from the request and validate
            try
            {
                //Create the host player who is joining, will perform data valiation inside Player Class
                var hostPlayer = new Player(request.nickname, request.imgUrl, request.contact);
                hostPlayer.IsHost = true;

                var gameDAL = new GameDAL();
                Response<Game> createdGame = null;
                Response<Player> createdPlayer = null;

                //Generate a game code and create a new game in the database
                //Creating the game inside a while loop because there is a chance that the gamecode could be duplicated
                var doRun = true;
                while(doRun)
                {
                    Game newGame = null;

                    newGame = new Game(Game.GenerateGameCode(),
                                    request.timeLimit,
                                    request.ammoLimit,
                                    request.startDelay,
                                    request.replenishAmmoDelay,
                                    request.gameMode,
                                    request.isJoinableAtAnytime,
                                    request.Latitude,
                                    request.Longitude,
                                    request.Radius);
                   
                    createdGame = gameDAL.CreateGame(newGame);

                    //If the response is successful the game was successfully created
                    if (createdGame.IsSuccessful())
                        doRun = false;

                    //If the response contains an error code of ITEMALREADYEXISTS then the game code is not unique,
                    //Want to run again and generate a new code
                    else if (createdGame.ErrorCode == ErrorCodes.ITEM_ALREADY_EXISTS)
                        doRun = true;

                    //If the response contains any other error code we do not want to run again because an un expected
                    //error occurred and we want to return the error response.
                    else
                        doRun = false;
                }

                //If the create game failed, return the error message and code from that response
                if (!createdGame.IsSuccessful())
                    return new Response<Player>(createdGame.ErrorMessage, createdGame.ErrorCode);

                //Call the player data access layer to join the player to that game
                var verificationCode = Player.GenerateVerificationCode();
                createdPlayer = new PlayerDAL().JoinGame(createdGame.Data, hostPlayer, verificationCode);

                //If the response was successful, send the verification code to the player
                var message = "Your CamTag verification code is: " + verificationCode;
                var subject = "CamTag Verification Code";
                if (createdPlayer.IsSuccessful())
                    createdPlayer.Data.ReceiveMessage(message, subject);

                //Otherwise, the host player failed to join the game deleted the created game
                else
                    gameDAL.DeactivateGameAfterHostJoinError(createdGame.Data);

                return createdPlayer;
            }
            //Catch any error associated with invalid model data
            catch (InvalidModelException e)
            {
                return new Response<Player>(e.Msg, e.Code);
            }
            //Catch any unhandled / unexpected server errrors
            catch
            {
                return StatusCode(500);
            }
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

            try
            {
                //Validate the filer and orderby value is a valid value
                if (string.IsNullOrWhiteSpace(filter) || string.IsNullOrWhiteSpace(orderBy))
                    return new Response<Game>("The filter or orderBy value is null or empty.", ErrorCodes.DATA_INVALID);

                //Confirm the filter value passed in is a valid value
                var isFilterValid = false;
                if (filter.ToUpper() == "ALL" || filter.ToUpper() == "ACTIVE" || filter.ToUpper() == "INGAME" || filter.ToUpper() == "INGAMEALL" || filter.ToUpper() == "TAGGABLE")
                    isFilterValid = true;

                //Confirm the order by value passed in is a valid value
                var isOrderByValid = false;
                if (orderBy.ToUpper() == "AZ" || orderBy.ToUpper() == "ZA" || orderBy.ToUpper() == "KILLS")
                    isOrderByValid = true;

                //If any of the values are invalid, set them to a default value
                if (!isFilterValid)
                    filter = "ALL";
                if (!isOrderByValid)
                    orderBy = "AZ";

                return new GameDAL().GetAllPlayersInGame(id, isPlayerID, filter, orderBy);
            }
            //Catch any error associated with invalid model data
            catch (InvalidModelException e)
            {
                return new Response<Game>(e.Msg, e.Code);
            }
            //Catch any unhandled / unexpected server errrors
            catch
            {
                return StatusCode(500);
            }
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
            try
            {
                var hostPlayer = new Player(playerID);
                var response = new GameDAL().BeginGame(hostPlayer);

                //If the response is successful schedule code to run to update the GameState after the time periods have passed
                if (response.IsSuccessful())
                {
                    var hubInterface = new HubInterface(_hubContext);
                    ScheduledTasks.ScheduleGameInPlayingState(response.Data, hubInterface);
                    ScheduledTasks.ScheduleCompleteGame(response.Data, hubInterface);

                    //Update all clients that the game is now in a starting state and the game will be playing soon
                    hubInterface.UpdateGameInStartingState(response.Data);
                }
                return response;
            }
            //Catch any error associated with invalid model data
            catch (InvalidModelException e)
            {
                return new Response<Game>(e.Msg, e.Code);
            }
            //Catch any unhandled / unexpected server errrors
            catch
            {
                return StatusCode(500);
            }
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
            try
            {
                var player = new Player(playerID);
                
                //Call the data access layer to get the status of the game / player
                return new GameDAL().GetGameStatus(player);
            }
            //Catch any error associated with invalid model data
            catch (InvalidModelException e)
            {
                return new Response<GameStatusResponse>(e.Msg, e.Code);
            }
            //Catch any unhandled / unexpected server errrors
            catch
            {
                return StatusCode(500);
            }
        }
    }
}
