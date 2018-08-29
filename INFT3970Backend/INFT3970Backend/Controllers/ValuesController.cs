using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using INFT3970Backend.Models;

namespace INFT3970Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<Response<Player>> Get()
        {
            Response<Player> response = new Response<Player>(new Player(), ResponseType.SUCCESS, null);

            return response;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        /*[HttpPost]
        public Player Post([FromBody] Player player)
        {
            player.Nickname = "Accepted at POST";
            return player;
        }*/


        [HttpPost]
        public void Post([FromBody] string value)
        {
            Console.WriteLine(value);
        }




        [HttpPut]
        public void Put([FromBody] string value)
        {
            Console.WriteLine(value);
        }



        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
