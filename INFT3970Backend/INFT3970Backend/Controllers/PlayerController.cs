using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using INFT3970Backend.Models;
using INFT3970Backend.Models.Requests;
using INFT3970Backend.Business_Logic_Layer;
using Microsoft.AspNetCore.SignalR;
using INFT3970Backend.Hubs;

namespace INFT3970Backend.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class PlayerController : ControllerBase
    {
        //The application hub context, used to be able to invokve client methods from anywhere in the code
        private readonly IHubContext<ApplicationHub> _hubContext;
        public PlayerController(IHubContext<ApplicationHub> hubContext)
        {
            _hubContext = hubContext;
        }



        /// <summary>
        /// POST: api/player/joinGame - Joins a player to a game matching the gameCode value, 
        /// creating a new Player record and returning the created Player object.
        /// </summary>
        /// <param name="gameCode">The gamecode the player is trying to join</param>
        /// <param name="nickname">The players nickname in the game</param>
        /// <param name="contact">The players contact info, either phone or email</param>
        /// <param name="imgUrl">The imgUrl of the players profile picture</param>
        /// <returns>Response including the created Player object. NULL data if an error occurred.</returns>
        [HttpPost]
        [Route("api/player/joinGame")]
        public ActionResult<Response<Player>> JoinGame(JoinGameRequest request)
        {
            //Example request:
            //Use POSTMAN and POST 'Form-Data' using the following values
            //Key               Value
            //gameCode          tcf124
            //nickname          billy
            //contact           enter an email or phone (NOTE: if using a phone it will send a text message to my number cause the twilio trial can only send to one number)
            //imgUrl            the imgUrl string of the players profile picture

            //Call the business logic layer to validate the form data and create a new player
            return new PlayerBL().JoinGame(request.gameCode, request.nickname, request.contact, request.imgUrl, false);
        }







        /// <summary>
        /// Verifies a player by checking the verification code they have entered is correct. 
        /// Returns a Response indicating success or failure.
        /// </summary>
        /// <param name="verificationCode">The verification code received by the user via phone or email.</param>
        /// <returns>Response with NULL data, outlining if the database update was SUCCESSFUL or ERROR</returns>
        [HttpPost]
        [Route("api/player/verify")]
        public Response<object> VerifyPlayer([FromForm] string verificationCode, [FromHeader] int playerID)
        {
            //Call the business logic to verify the player and return a SUCCESS or ERROR response
            return new PlayerBL().VerifyPlayer(verificationCode, playerID, _hubContext);
        }






        /// <summary>
        /// Generates a new verification code for the player, updates the verification code in the database
        /// and resends the new code to the player's contact information (Email or Phone).
        /// </summary>
        /// <param name="playerID">The playerID who's verification code is being updated</param>
        /// <returns>Response with NULL data, outlining if the database update was SUCCESSFUL or ERROR</returns>
        [HttpPost]
        [Route("api/player/resend")]
        public Response<object> ResendVerificationCode([FromHeader] int playerID)
        {
            //Call the business logic layer
            return new PlayerBL().ResendVerificationCode(playerID);
        }




        /// <summary>
        /// GET: api/player/getNotifications/{playerID}/{all} - 
        /// Gets a list of all the notifications associated with a particular player
        /// </summary>
        /// <param name="playerID">The playerID used to determine which player the notifications are for</param>
        /// <param name="all"> all is a boolean used to determine if all notifications should be fetched or just unread</param>
        /// <returns>A list of notifications for a player</returns>
        [HttpGet]
        [Route("api/player/getNotifications/{playerID:int}/{all:bool}")]
        public Response<List<Notification>> GetNotificationList(int playerID, bool all)
        {
            //Example request
            //https://localhost:5000/api/player/getNotifications/100000/false

            PlayerBL playerBL = new PlayerBL();
            return playerBL.GetNotificationList(playerID, all);
        }




        /// <summary>
        /// POST: api/player/leaveGame - 
        /// Leaves a player from their active game
        /// </summary>
        /// <param name="playerID">The playerID used to determine which player is leaving the game.</param>
        /// <returns>A response status indicating if the db update was a success.</returns>
        [HttpPost]
        [Route("api/player/leaveGame")]
        public Response<object> LeaveGame([FromHeader] int playerID)
        {
            //Example request
            //https://localhost:5000/api/player/leaveGame

            PlayerBL playerBL = new PlayerBL();
            return playerBL.LeaveGame(playerID, _hubContext);
        }



        /// <summary>
        /// POST: api/player/notificationsRead
        /// Marks a set of player notifications as read
        /// </summary>
        /// <param name="jsonNotificationIDs">The playerID and notificationIDs to mark as read.</param>
        /// <returns>A response status indicating if the db update was a success.</returns>
        [HttpPost]
        [Route("api/player/setNotificationsRead")]
        public Response<object> SetNotificationsRead(ReadNotificationsRequest jsonNotificationIDs)
        {
            //Example request
            //https://localhost:5000/api/player/setNotificationsRead

            PlayerBL playerBL = new PlayerBL();
            return playerBL.SetNotificationsRead(jsonNotificationIDs);
        }






        /// <summary>
        /// Uses the Players ammo, decrements the players ammo count and schedules the ammo to replenish
        /// after a certain time period.
        /// </summary>
        /// <param name="playerID">The ID of the Player</param>
        /// <returns>The updated Player object after the ammo count is decremented. NULL if error</returns>
        [HttpPost]
        [Route("api/player/useAmmo")]
        public Response<Player> UseAmmo([FromHeader] int playerID)
        {
            //Example request
            //https://localhost:5000/api/player/useAmmo

            PlayerBL playerBL = new PlayerBL();
            return playerBL.UseAmmo(playerID, _hubContext);
        }






        /// <summary>
        /// Gets the ammo count for the Player
        /// </summary>
        /// <param name="playerID">The ID of the player</param>
        /// <returns>The ammo count, negative INT if error</returns>
        [HttpGet]
        [Route("api/player/ammo")]
        public Response<int> GetAmmoCount([FromHeader] int playerID)
        {
            //Example request
            //https://localhost:5000/api/player/ammo

            PlayerBL playerBL = new PlayerBL();
            return playerBL.GetAmmoCount(playerID);
        }
    }
}
