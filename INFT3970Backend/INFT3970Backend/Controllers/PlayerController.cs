using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using INFT3970Backend.Models;
using INFT3970Backend.Models.Requests;
using Microsoft.AspNetCore.SignalR;
using INFT3970Backend.Hubs;
using INFT3970Backend.Models.Errors;
using INFT3970Backend.Data_Access_Layer;
using INFT3970Backend.Helpers;

namespace INFT3970Backend.Controllers
{
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
        /// Joins a new player to a game matching the game code passed in. Stores their player details 
        /// and sends them a verification code once they have joined the game.
        /// Returns the created Player object. NULL data if error occurred.
        /// </summary>
        /// <param name="request">The request which contains the player information and the GameCode to join.</param>
        /// <returns>Returns the created Player object. NULL data if error occurred.</returns>
        [HttpPost]
        [Route("api/player/joinGame")]
        public ActionResult<Response<Player>> JoinGame(JoinGameRequest request)
        {
            try
            {
                //Create the player object who will be joining the game
                var playerToJoin = new Player(request.nickname, request.imgUrl, request.contact);
                var gameToJoin = new Game(request.gameCode);

                //Generate a verification code 
                var verificationCode = Player.GenerateVerificationCode();

                //Call the data access layer to add the player to the database
                var response = new PlayerDAL().JoinGame(gameToJoin, playerToJoin, verificationCode);

                //If the response was successful, send the verification code to the player and update the lobby list
                if (response.IsSuccessful())
                {
                    var message = "Your CamTag verification code is: " + verificationCode;
                    var subject = "CamTag Verification Code";
                    response.Data.ReceiveMessage(message, subject);

                    //Call the hub interface to invoke client methods to update the clients that another player has joined
                    var hubInterface = new HubInterface(_hubContext);
                    hubInterface.UpdatePlayerJoinedGame(response.Data);
                }
                return response;
            }
            //Catch any error associated with invalid model data
            catch (InvalidModelException e)
            {
                return new Response<Player>(e.Msg, e.Code);
            }
            //Catch any unhandled / unexpected server errrors
            catch
            {
                return StatusCode(500);
            }
        }







        /// <summary>
        /// Validates a players received verification code. If the player successfully enters their 
        /// verification code their player record will be verified, meaning they have confirmed their 
        /// email address or phone number is correct and has access to it throughout the game.
        /// </summary>
        /// <param name="verificationCode">The code received and entered by the player</param>
        /// <param name="playerID">The playerID trying to verify</param>
        /// <returns>SUCCESS or ERROR response.</returns>
        [HttpPost]
        [Route("api/player/verify")]
        public ActionResult<Response> VerifyPlayer([FromForm] string verificationCode, [FromHeader] int playerID)
        {
            try
            {
                var playerToVerify = new Player(playerID);

                //Confirm the verification code is valid and return an error response if the verification code is invalid
                var code = Player.ValidateVerificationCode(verificationCode);
                if(code == -1)
                    return new Response("The verification code is invalid. Must be an INT between 10000 and 99999.", ErrorCodes.DATA_INVALID);

                //Call the data access layer to confirm the verification code is correct.
                var response = new PlayerDAL().ValidateVerificationCode(code, playerToVerify);

                //If the player was successfully verified, updated all the clients about a joined player.
                if (response.IsSuccessful())
                {
                    var hubInterface = new HubInterface(_hubContext);
                    hubInterface.UpdatePlayerJoinedGame(response.Data);
                }

                return response;
            }
            //Catch any error associated with invalid model data
            catch (InvalidModelException e)
            {
                return new Response(e.Msg, e.Code);
            }
            //Catch any unhandled / unexpected server errrors
            catch
            {
                return StatusCode(500);
            }
        }






        /// <summary>
        /// Generates a new verification code for the player, updates the verification code in the database
        /// and resends the new code to the player's contact information (Email or Phone).
        /// </summary>
        /// <param name="playerID">The playerID who's verification code is being updated</param>
        /// <returns>SUCCESSFUL or ERROR response.</returns>
        [HttpPost]
        [Route("api/player/resend")]
        public ActionResult<Response> ResendVerificationCode([FromHeader] int playerID)
        {
            try
            {
                var player = new Player(playerID);

                //Generate a new code
                var newCode = Player.GenerateVerificationCode();

                //Call the Data Access Layer to update the verification code for the player and 
                //get the contact information for the player
                var response = new PlayerDAL().UpdateVerificationCode(player, newCode);

                //If the response is successful send the verification code to the player
                var msgTxt = "Your CamTag verification code is: " + newCode;
                var subject = "CamTag Verification Code";
                if (response.IsSuccessful())
                    response.Data.ReceiveMessage(msgTxt, subject);

                return new Response(response.ErrorMessage, response.ErrorCode);
            }
            //Catch any error associated with invalid model data
            catch (InvalidModelException e)
            {
                return new Response(e.Msg, e.Code);
            }
            //Catch any unhandled / unexpected server errrors
            catch
            {
                return StatusCode(500);
            }
        }




        /// <summary>
        /// Gets a list of all the notifications associated with a particular player
        /// </summary>
        /// <param name="playerID">The playerID used to determine which player the notifications are for</param>
        /// <param name="all"> all is a boolean used to determine if all notifications should be fetched or just unread</param>
        /// <returns>A list of notifications for a player</returns>
        [HttpGet]
        [Route("api/player/getNotifications/{playerID:int}/{all:bool}")]
        public ActionResult<Response<List<Notification>>> GetNotificationList(int playerID, bool all)
        {
            try
            {
                //Call the data access layer to return the notifications for the player.
                var player = new Player(playerID);
                return new PlayerDAL().GetNotificationList(player, all);
            }
            //Catch any error associated with invalid model data
            catch (InvalidModelException e)
            {
                return new Response<List<Notification>>(e.Msg, e.Code);
            }
            //Catch any unhandled / unexpected server errrors
            catch
            {
                return StatusCode(500);
            }
        }




        /// <summary>
        /// Leaves a player from their active game
        /// </summary>
        /// <param name="playerID">The playerID used to determine which player is leaving the game.</param>
        /// <returns>A response status indicating if the db update was a success.</returns>
        [HttpPost]
        [Route("api/player/leaveGame")]
        public ActionResult<Response> LeaveGame([FromHeader] int playerID)
        {
            //TODO: leave game notification during STARTING state
            //TODO: Update lobby list when the player leaves game IN LOBBY and STARTING
            try
            {
                var player = new Player(playerID);

                //Call the data access layer to remove the player from the game.
                var playerDAL = new PlayerDAL();
                var isGameComplete = false;
                var isPhotosComplete = false;
                var leaveGameResponse = playerDAL.LeaveGame(player, ref isGameComplete, ref isPhotosComplete);

                //Return the error response if an error occurred
                if (!leaveGameResponse.IsSuccessful())
                    return new Response(leaveGameResponse.ErrorMessage, leaveGameResponse.ErrorCode);

                //Get the updated player object from the database
                var playerResponse = playerDAL.GetPlayerByID(player.PlayerID);
                if (!playerResponse.IsSuccessful())
                    return new Response(playerResponse.ErrorMessage, playerResponse.ErrorCode);

                //Create the hub interface which will be used to send live updates to clients
                var hubInterface = new HubInterface(_hubContext);

                //Call the hub method to send out notifications to players that the game is now complete
                if (isGameComplete)
                    hubInterface.UpdateGameCompleted(playerResponse.Data.Game, true);

                //Otherwise, if the photo list is not empty then photos have been completed and need to send out updates
                else if (isPhotosComplete)
                {
                    foreach (var photo in leaveGameResponse.Data)
                        hubInterface.UpdatePhotoVotingCompleted(photo);
                }

                //If the game is not completed send out the player left notification
                if (!isGameComplete)
                    hubInterface.UpdatePlayerLeft(playerResponse.Data);

                return new Response(leaveGameResponse.ErrorMessage, leaveGameResponse.ErrorCode);
            }
            //Catch any error associated with invalid model data
            catch (InvalidModelException e)
            {
                return new Response(e.Msg, e.Code);
            }
            //Catch any unhandled / unexpected server errrors
            catch
            {
                return StatusCode(500);
            }
        }



        /// <summary>
        /// Marks a set of player notifications as read
        /// </summary>
        /// <param name="jsonNotificationIDs">The playerID and notificationIDs to mark as read.</param>
        /// <returns>A response status indicating if the db update was a success.</returns>
        [HttpPost]
        [Route("api/player/setNotificationsRead")]
        public ActionResult<Response> SetNotificationsRead(ReadNotificationsRequest jsonNotificationIDs)
        {
            try
            {
                //Call the data access layer to update the notification records
                return new PlayerDAL().SetNotificationsRead(jsonNotificationIDs);
            }
            //Catch any error associated with invalid model data
            catch (InvalidModelException e)
            {
                return new Response(e.Msg, e.Code);
            }
            //Catch any unhandled / unexpected server errrors
            catch
            {
                return StatusCode(500);
            }
        }






        /// <summary>
        /// Uses the Players ammo, decrements the players ammo count and schedules the ammo to replenish
        /// after a certain time period.
        /// </summary>
        /// <param name="playerID">The ID of the Player</param>
        /// <returns>The updated Player object after the ammo count is decremented. NULL if error</returns>
        [HttpPost]
        [Route("api/player/useAmmo")]
        public ActionResult<Response<Player>> UseAmmo([FromHeader] int playerID)
        {
            try
            {
                //Call the DataAccessLayer to update the Ammo count for the player
                var player = new Player(playerID);
                var response = new PlayerDAL().UseAmmo(player);

                //If the response was successful schedule code to run in order to replenish the players ammo
                if (response.IsSuccessful())
                {
                    var hubInterface = new HubInterface(_hubContext);
                    ScheduledTasks.ScheduleReplenishAmmo(response.Data, hubInterface);
                }
                return response;
            }
            //Catch any error associated with invalid model data
            catch (InvalidModelException e)
            {
                return new Response<Player>(e.Msg, e.Code);
            }
            //Catch any unhandled / unexpected server errrors
            catch
            {
                return StatusCode(500);
            }
        }






        /// <summary>
        /// Gets the ammo count for the Player
        /// </summary>
        /// <param name="playerID">The ID of the player</param>
        /// <returns>The ammo count, negative INT if error</returns>
        [HttpGet]
        [Route("api/player/ammo")]
        public ActionResult<Response<int>> GetAmmoCount([FromHeader] int playerID)
        {
            try
            {
                var player = new Player(playerID);
                return new PlayerDAL().GetAmmoCount(player);
            }
            //Catch any error associated with invalid model data
            catch (InvalidModelException e)
            {
                return new Response<int>(e.Msg, e.Code);
            }
            //Catch any unhandled / unexpected server errrors
            catch
            {
                return StatusCode(500);
            }
        }





        /// <summary>
        /// Gets the count of unread notifications for the player.
        /// </summary>
        /// <param name="playerID">The ID of the player</param>
        /// <returns>The count of unread notifications, negative INT if error</returns>
        [HttpGet]
        [Route("api/player/unread")]
        public ActionResult<Response<int>> GetUnreadNotificationCount([FromHeader] int playerID)
        {
            try
            {
                var player = new Player(playerID);
                return new PlayerDAL().GetUnreadNotificationsCount(player);
            }
            //Catch any error associated with invalid model data
            catch (InvalidModelException e)
            {
                return new Response<int>(e.Msg, e.Code);
            }
            //Catch any unhandled / unexpected server errrors
            catch
            {
                return StatusCode(500);
            }
        }

    }
}
