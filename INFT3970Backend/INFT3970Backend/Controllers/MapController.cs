using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using INFT3970Backend.Models;
using INFT3970Backend.Business_Logic_Layer;
using Microsoft.AspNetCore.Mvc;
using INFT3970Backend.Models.Errors;

namespace INFT3970Backend.Controllers
{
    [ApiController]
    public class Map : ControllerBase
    {
        [HttpGet]
        [Route("api/map/getLastPhotoLocations/{playerID:int}")]
        public ActionResult<Response<List<Photo>>> GetLastPhotoLocations(int playerID)
        {
            try
            {
                var player = new Player(playerID);
                return new PhotoBL().GetLastKnownLocations(player);
            }
            //Catch any error associated with invalid model data
            catch (InvalidModelException e)
            {
                return new Response<List<Photo>>(e.Msg, e.Code);
            }
            //Catch any unhandled / unexpected server errrors
            catch
            {
                return StatusCode(500);
            }
        }
    }

   
}

