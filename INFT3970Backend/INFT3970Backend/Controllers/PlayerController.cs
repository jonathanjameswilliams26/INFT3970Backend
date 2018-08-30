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
