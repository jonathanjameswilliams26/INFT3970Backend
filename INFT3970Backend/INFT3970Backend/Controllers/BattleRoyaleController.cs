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
    public class BattleRoyaleController : ControllerBase
    {
        //The application hub context, used to be able to invokve client methods from anywhere in the code
        private readonly IHubContext<ApplicationHub> _hubContext;
        public BattleRoyaleController(IHubContext<ApplicationHub> hubContext)
        {
            _hubContext = hubContext;
        }



        /// <summary>
        /// Uses the Players ammo, decrements the players ammo count and schedules the ammo to replenish.
        /// Checks to see if the player is inside the zone. If the player is not inside the zone the players
        /// "lives" are decremented, if the lives reach 0 the player is eliminated from the game.
        /// </summary>
        /// <param name="playerID">The ID of the Player</param>
        /// <returns>The updated Player object after the ammo count is decremented. NULL if error</returns>
        [HttpPost]
        [Route("api/br/useAmmo")]
        public ActionResult<Response<BRPlayer>> UseAmmo([FromHeader] int playerID, [FromForm] double latitude, [FromForm] double longitude)
        {
            try
            {
                var player = new Player(playerID);
                var DAL = new BattleRoyaleDAL();

                //Use ammo and return the updated BRPlayer from the DB
                var response = DAL.BR_UseAmmo(player);
                if (!response.IsSuccessful())
                    return response;

                var brPlayer = response.Data;

                //Confirm the player is within the zone
                if(brPlayer.Game.IsInZone(latitude, longitude))
                {
                    //Schedule the ammo to replenish and return the response
                    var hubInterface = new CoreHubInterface(_hubContext);
                    ScheduledTasks.ScheduleReplenishAmmo(response.Data, hubInterface);
                    return response;
                }

                //Otherwise, the player is not within the zone, possibly eliminate from game
                else
                {
                    //Decrement the number of lives the player has
                    response = DAL.BR_DecreaseLives(brPlayer);
                    if (!response.IsSuccessful())
                        return response;

                    brPlayer = response.Data;

                    //If the player is no longer alive because too many photos where taken outside the zone
                    //eliminate from the game
                    if(!brPlayer.IsAlive())
                    {
                        response = DAL.BR_EliminatePlayer(brPlayer);

                        if (!response.IsSuccessful())
                            return response;

                        //Call the hub interface to update all the clients that a player has been eliminated
                        var hubInterface = new BRHubInterface(_hubContext);
                        hubInterface.UpdatePlayerEliminated(brPlayer);

                        //Return an error code to indicate the player has been eliminated
                        return new Response<BRPlayer>("You have been eliminated", ErrorCodes.USEAMMO_PLAYERELIMINATED);
                    }

                    //Otherwise, the player is still in the game after taking a photo outside the zone
                    //Schedule the ammo to be replenished and return an error response to indicate the player is not inside the zone
                    else
                    {
                        var hubInterface = new CoreHubInterface(_hubContext);
                        ScheduledTasks.ScheduleReplenishAmmo(response.Data, hubInterface);

                        response.ErrorMessage = "You are outside the zone, cannot take photo.";
                        response.ErrorCode = ErrorCodes.USEAMMO_NOTINZONE;
                        return response;
                    }
                }
            }
            //Catch any error associated with invalid model data
            catch (InvalidModelException e)
            {
                return new Response<BRPlayer>(e.Msg, e.Code);
            }
            //Catch any unhandled / unexpected server errrors
            catch
            {
                return StatusCode(500);
            }
        }
    }
}
