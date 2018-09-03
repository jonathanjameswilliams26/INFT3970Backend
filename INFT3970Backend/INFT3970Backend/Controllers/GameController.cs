using INFT3970Backend.Business_Logic_Layer;
using INFT3970Backend.Hubs;
using INFT3970Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;


namespace INFT3970Backend.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        // GET: api/Game
        /*[HttpGet]
        public ActionResult<Response<Game>> Get()
        {
            return Ok();
        }*/



        /// <summary>
        /// POST: api/player/createGame - Creates a new game and joins a player to their created game, creating a new Player record and returning the created PlayerID
        /// </summary>
        /// <param name="nickname">The players nickname in the game</param>
        /// <param name="contact">The players contact info, either phone or email</param>
        /// <returns>Response including the playerID and gameCode generated in the database. If an error occurs then a negative PlayerID is returned in the response</returns>
        [HttpPost]
        [Route("api/player/createGame")]
        public Response<int> CreateGame([FromForm] string nickname, [FromForm] string contact) //settings?
        {
            //Example request:
            //Use POSTMAN and POST 'Form-Data' using the following values
            //Key               Value
            //nickname          billy
            //contact           enter an email or phone (NOTE: if using a phone it will send a text message to my number cause the twilio trial can only send to one number)
            //various settings??

            GameBL gameBL = new GameBL();
            Response<string> response = gameBL.CreateGame();
            string gameCode = response.Data;


            //Call the business logic layer to validate the form data and create a new host player
            PlayerBL playerBL = new PlayerBL();
            Response<int> response2 = playerBL.JoinGame(gameCode, nickname, contact);


            return response2;
        }
    }






}
