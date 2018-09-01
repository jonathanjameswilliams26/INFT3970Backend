using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using INFT3970Backend.Data_Access_Layer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using INFT3970Backend.Models;
using INFT3970Backend.Business_Logic_Layer;

namespace INFT3970Backend.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class PlayerController : ControllerBase
    {


        /// <summary>
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
        /// POST: api/Player/SetConnectionID
        /// </summary>
        /// <param name="PlayerIDAndConnectionID">Key Value pair of JSON body data representing the PlayerID and their connectionID to the hub</param>
        /// <returns>Response Object outlining if the database update was SUCCESSFUL or ERROR</returns>
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

    }
}
