using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using INFT3970Backend.Models;
using INFT3970Backend.Business_Logic_Layer;
using Microsoft.AspNetCore.Mvc;

namespace INFT3970Backend.Controllers
{
    [ApiController]
    public class Map : ControllerBase
    {
        [HttpGet]
        [Route("api/map/getLastPhotoLocations/{playerID:int}")]
        public ActionResult<Response<List<Photo>>> GetLastPhotoLocations(int playerID)
        {
         
            PhotoBL photoBL = new PhotoBL();
            return photoBL.GetLastKnownLocations(playerID);

        }
    }

   
}

