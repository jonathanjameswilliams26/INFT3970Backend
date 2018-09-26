using System.Collections.Generic;
using INFT3970Backend.Models;
using Microsoft.AspNetCore.Mvc;
using INFT3970Backend.Models.Errors;
using INFT3970Backend.Data_Access_Layer;

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
                
                //Call the data access layer to get the last known locations
                return new PhotoDAL().GetLastKnownLocations(player);
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

