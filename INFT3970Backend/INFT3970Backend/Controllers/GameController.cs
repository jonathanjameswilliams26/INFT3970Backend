using Microsoft.AspNetCore.Mvc;
using INFT3970Backend.Models;

namespace INFT3970Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        // GET: api/Game
        [HttpGet]
        public ActionResult<Response<Game>> Get()
        {
            return Ok();
        }

       
    }
}
