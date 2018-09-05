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
        [Route("api/player/getPhotoLocation/{photoID:int}")]
        public ActionResult<Response<List<Photo>>> GetPhotoLocation(int photoID)
        {
            /*
            int lattitude = -24;
            int longitude = 130;

            Location marker = new Location();
            marker.Lattitude = lattitude;
            marker.Longitude = longitude;

            List<Location> _locations = new List<Location>();
            _locations.Add(marker);

            
            return new Response<List<Location>>(_locations, ResponseType.SUCCESS, "", 1);
             */
            PhotoBL photoBL = new PhotoBL();
            return photoBL.GetPhotoLocation(photoID);

        }
    }

   
}

