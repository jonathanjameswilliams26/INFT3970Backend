using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using INFT3970Backend.Business_Logic_Layer;
using INFT3970Backend.Hubs;
using INFT3970Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace INFT3970Backend.Controllers
{
    
    [ApiController]
    public class PhotoController : ControllerBase
    {

        //The application hub context, used to be able to invokve client methods from anywhere in the code
        private readonly IHubContext<ApplicationHub> _hubContext;
        public PhotoController(IHubContext<ApplicationHub> hubContext)
        {
            _hubContext = hubContext;
        }


        // GET: api/Photo
        [HttpPost]
        [Route("api/photo/upload")]
        public ActionResult<Response<Photo>> Upload([FromForm] string imgUrl, [FromForm] string takenByID, [FromForm] string photoOfID)
        {
            int id1 = int.Parse(takenByID);
            int id2 = int.Parse(photoOfID);
            PhotoBL photoBL = new PhotoBL();
            return photoBL.SavePhoto(imgUrl, id1, id2, _hubContext);
        }
    }
}
