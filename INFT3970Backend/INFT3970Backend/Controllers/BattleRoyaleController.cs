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
        public ActionResult<Response<Player>> UseAmmo([FromHeader] int playerID, [FromForm] double latitude, [FromForm] double longitude)
        {
            try
            {
                return null;
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
    }
}
