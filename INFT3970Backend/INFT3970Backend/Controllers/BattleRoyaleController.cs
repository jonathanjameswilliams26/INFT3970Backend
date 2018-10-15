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
                var DAL = new BattleRoyaleDAL();

                //Get the player object from the database
                var getPlayerResponse = DAL.BR_GetPlayerByID(playerID);

                //If the response was not successful return an error
                if (!getPlayerResponse.IsSuccessful())
                    return getPlayerResponse;

                var player = getPlayerResponse.Data;
                var game = player.Game;

                //Confirm the player can actually use ammo
                if (player.AmmoCount == 0)
                    return new Response<BRPlayer>("Cannot use ammo because ammo count is 0.", ErrorCodes.CANNOT_PERFORM_ACTION);

                //Confirm the player is within the playing zone
                if(game.IsInZone(latitude, longitude))
                {
                    //Use ammo because the player is within the zone
                    var useAmmoResponse = DAL.BR_UseAmmo(player);

                    //If the use ammo request was successful schedule the ammo to be replenished
                    if(useAmmoResponse.IsSuccessful())
                    {
                        var hubInterface = new HubInterface(_hubContext);
                        ScheduledTasks.ScheduleReplenishAmmo(useAmmoResponse.Data, hubInterface);
                    }
                    return useAmmoResponse;
                }

                //Otherwise, the player is not within the zone, reduce lives and possibly eliminate from game
                else
                {
                    player.LivesRemaining--;

                    //If the player is no longer alive then eliminate the player from the game
                    if (!player.IsAlive())
                    {
                        var eliminatePlayerResponse = DAL.BR_EliminatePlayer(player);

                        //If the player was successfully eliminated send out updates to clients
                        if (eliminatePlayerResponse.IsSuccessful())
                        {
                            var hubInterface = new HubInterface(_hubContext);
                            hubInterface.UpdatePlayerEliminated(_hubContext);
                        }
                        return eliminatePlayerResponse;
                    }

                    //Otherwise, return an error to indicate that the player is outside the playing area
                    else
                        return new Response<BRPlayer>("Not inside the zone.", ErrorCodes.DATA_INVALID);
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
