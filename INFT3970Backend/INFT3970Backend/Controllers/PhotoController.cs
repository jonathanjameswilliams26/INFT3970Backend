using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using INFT3970Backend.Business_Logic_Layer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace INFT3970Backend.Controllers
{
    
    [ApiController]
    public class PhotoController : ControllerBase
    {
        // GET: api/Photo
        [HttpPost]
        [Route("api/photo/upload")]
        public ActionResult Upload([FromForm] string imgUrl)
        {
            PhotoBL photoBL = new PhotoBL();
            photoBL.SavePhoto(imgUrl);
            return Ok();
        }
    }
}
