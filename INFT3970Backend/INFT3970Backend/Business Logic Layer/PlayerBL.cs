using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using INFT3970Backend.Data_Access_Layer;
using INFT3970Backend.Models;
using INFT3970Backend.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace INFT3970Backend.Business_Logic_Layer
{
    public class PlayerBL
    {

        /// <summary>
        /// Updates the players connection ID. Returns NULL data.
        /// </summary>
        /// <param name="playerID">The PlayerID being updated</param>
        /// <param name="connectionID">The new ConnectionID</param>
        /// <returns></returns>
        public void UpdateConnectionID(int playerID, string connectionID)
        {
            //Call the Data Access Layer to update the playerID's connectionID in the Database
            PlayerDAL playerDAL = new PlayerDAL();
            playerDAL.UpdateConnectionID(playerID, connectionID);
        }







        /// <summary>
        /// Joins a new player to a game matching the game code passed in. Stores their player details 
        /// and sends them a verification code once they have joined the game.
        /// Returns the created Player object. NULL data if error occurred.
        /// </summary>
        /// <param name="gameCode">The game the player is trying to join.</param>
        /// <param name="nickname">The players nickname in the game</param>
        /// <param name="contact">The players contact either phone or email where the player will be contacted throughout the game.</param>
        /// <param name="isHost">A flag which outlines if the player joining the game is the host of the game.</param>
        /// <returns>Returns the created Player object. NULL data if error occurred.</returns>
        public Response<Player> JoinGame(Game game, Player player)
        {
            //Call the data access layer to add the player to the database
            int verificationCode = GenerateVerificationCode();
            PlayerDAL playerDAL = new PlayerDAL();
            Response<Player> response = playerDAL.JoinGame(game, player, verificationCode);

            //If the response was successful, send the verification code to the player
            if(response.IsSuccessful())
            {
                //Send the verification code to the players email
                if(response.Data.HasEmail())
                    EmailSender.SendInBackground(response.Data.Email, "CamTag Verification Code", "Your CamTag verification code is: " + verificationCode, false);

                //otherwise, send the verification code to the players phone number
                else
                    TextMessageSender.SendInBackground("Your CamTag verification code is: " + verificationCode, response.Data.Phone);
            }

            return response;
        }







        /// <summary>
        /// Validates a players received verification code. If the player successfully enters their 
        /// verification code their player record will be verified, meaning they have confirmed their 
        /// email address or phone number is correct and has access to it throughout the game.
        /// </summary>
        /// <param name="verificationCode">The code received and entered by the player</param>
        /// <param name="playerID">The playerID trying to verify</param>
        /// <param name="hubContext">
        /// The context of the application hub which is used to send live updates via SignalR.
        /// This parameter will be used to updated all connected clients in the game that a new player has successfully joined.
        /// </param>
        /// <returns></returns>
        public Response<object> VerifyPlayer(string verificationCode, int playerID, IHubContext<ApplicationHub> hubContext)
        {
            //Confirm the verification code is not empty or null, and confirm the playerID is a valid INT, must be greater than 100000
            if (String.IsNullOrWhiteSpace(verificationCode) || playerID < 10000)
                return new Response<object>(null, "ERROR", "Missing request data, verification code is empty or null or playerID was not valid.", ErrorCodes.EC_MISSINGORBLANKDATA);


            //Confirm the verification code is a valid verification code, will always be an integer from 10000 - 99999
            int code = 0;
            try
            {
                //Confirm the code is within the valid range
                code = int.Parse(verificationCode);
                if (code < 10000 || code > 99999)
                    throw new Exception();
            }
            catch
            {
                return new Response<object>(null, "ERROR", "The verification code is invalid. Must be an INT between 10000 and 99999.", ErrorCodes.EC_VERIFYPLAYER_CODEINVALID);
            }

            //Call the data access layer to confirm the verification code is correct.
            PlayerDAL playerDAL = new PlayerDAL();
            Response<object> response = playerDAL.ValidateVerificationCode(code, playerID);

            //If the player was successfully verified, updated all the clients about a joined player.
            if(response.Type == "SUCCESS")
            {
                HubInterface hubInterface = new HubInterface(hubContext);
                hubInterface.UpdatePlayerJoined(playerID);
            }

            return response;
        }






        /// <summary>
        /// Gets the Player's ammo count
        /// </summary>
        /// <param name="playerID">The player which getting the ammo count for.</param>
        /// <returns>The ammo count, negative INT if error</returns>
        public Response<int> GetAmmoCount(int playerID)
        {
            //Call the DataAccessLayer to get the Ammo Count
            PlayerDAL playerDAL = new PlayerDAL();
            return playerDAL.GetAmmoCount(playerID);
        }










        /// <summary>
        /// Regenerates a verification code, updates the player record to store the new code and sends 
        /// the new code to the players contact (Email or Phone). Returns a response with NULL data, 
        /// use Response.Type to determine error or success.
        /// </summary>
        /// <param name="playerID">The player who's verification code is being updated.</param>
        /// <returns>NULL data, use Response.Type to confirm SUCCESS or ERROR</returns>
        public Response<object> ResendVerificationCode(int playerID)
        {
            //Generate a new verification code to send
            int code = GenerateVerificationCode();

            //Call the Data Access Layer to update the verification code for the player and get the contact information for the player
            Response<Player> response = new PlayerDAL().UpdateVerificationCode(playerID, code);

            //If the response is successful send the verification code
            if (response.Type == "SUCCESS")
            {
                //The contact information returned from the data access is an email address
                if (!string.IsNullOrWhiteSpace(response.Data.Email))
                    EmailSender.SendInBackground(response.Data.Email, "CamTag Verification Code", "Your CamTag verification code is: " + code, false);

                //Otherwise, the contact information returned is a phone number
                else
                    TextMessageSender.SendInBackground("Your CamTag verification code is: " + code, response.Data.Phone);

                return new Response<object>(null, "SUCCESS", null, 1);
            }
            //Otherwise, an error occurred while updating the validation code, return the error obtained from the database
            else
                return new Response<object>(null, "ERROR", response.ErrorMessage, response.ErrorCode);
        }






        /// <summary>
        /// Generates a 5 digit verification code
        /// </summary>
        /// <returns></returns>
        private int GenerateVerificationCode()
        {
            //Generate a verification code for the player to verify their contact details (5 digit verification code)
            Random rand = new Random();
            return rand.Next(10000, 99999);
        }




        /// <summary>
        /// Gets all notifications for the player
        /// </summary>
        /// <param name="playerID">The PlayerID of the notifications</param>
        /// <returns>A list of all player notifications</returns>
        public Response<List<Notification>> GetNotificationList(int playerID, bool all)
        {
            //Call the Data Access Layer to return all notifications for the player.
            PlayerDAL playerDAL = new PlayerDAL();
            return playerDAL.GetNotificationList(playerID, all);
        }


        /// <summary>
        /// Leaves a player from their active game.
        /// </summary>
        /// <param name="playerID">The playerID used to determine which player is leaving the game.</param>
        /// <returns>A response which indicates success or failure.</returns>
        public Response<object> LeaveGame(int playerID, IHubContext<ApplicationHub> hubContext)
        {
            //Call the Data Access Layer to remove a player from the game.
            PlayerDAL playerDAL = new PlayerDAL();
            bool isGameComplete = false;
            bool isPhotosComplete = false;
            var response = playerDAL.LeaveGame(playerID, ref isGameComplete, ref isPhotosComplete);

            //Return the error response if an error occurred
            if (!response.IsSuccessful())
                return new Response<object>(null, response.Type, response.ErrorMessage, response.ErrorCode);

            //Get the player object from the database
            var playerResponse = playerDAL.GetPlayerByID(playerID);
            if(!playerResponse.IsSuccessful())
                return new Response<object>(null, playerResponse.Type, playerResponse.ErrorMessage, playerResponse.ErrorCode);

            //Create the hub interface which will be used to send live updates to clients
            HubInterface hubInterface = new HubInterface(hubContext);

            //Call the hub method to send out notifications to players that the game is now complete
            if (isGameComplete)
                hubInterface.UpdateGameCompleted(playerResponse.Data.Game, true);

            //Otherwise, if the photo list is not empty then photos have been completed and need to send out updates
            else if(isPhotosComplete)
            {
                foreach(var photo in response.Data)
                    hubInterface.UpdatePhotoVotingCompleted(photo);
            }

            //If the game is not completed send out the player left notification
            if (!isGameComplete)
                hubInterface.UpdatePlayerLeft(playerID);

            return new Response<object>(null, response.Type, response.ErrorMessage, response.ErrorCode);
        }




        /// <summary>
        /// Marks player notifications as read.
        /// </summary>
        /// <param name="playerID">The playerID used to determine which player is leaving the game.</param>
        /// <returns>A response status.</returns>
        public Response<object> SetNotificationsRead(ReadNotificationsRequest jsonNotificationIDs)
        {                  
            //Call the Data Access Layer to remove a player from the game.
            PlayerDAL playerDAL = new PlayerDAL();
            return playerDAL.SetNotificationsRead(jsonNotificationIDs);
        }






        /// <summary>
        /// Uses a players ammo when the player takes a photo.
        /// The players ammo count will be decremented and a will be scheduled to be replenished after a certain timeframe.
        /// </summary>
        /// <param name="playerID">The playerID who's ammo count is being reduced.</param>
        /// <param name="hubContext">The IHubContext which will be used to send live updates to clients.</param>
        /// <returns>The updated player object after the Player record has been updated in the database. NULL if error occurred.</returns>
        public Response<Player> UseAmmo(int playerID, IHubContext<ApplicationHub> hubContext)
        {
            //Call the DataAccessLayer to update the Ammo count for the player
            PlayerDAL playerDAL = new PlayerDAL();
            Response<Player> response = playerDAL.UseAmmo(playerID);

            //If the response was successful schedule code to run in order to replenish the players ammo
            if (response.IsSuccessful())
            {
                HubInterface hubInterface = new HubInterface(hubContext);
                ScheduledTasks.ScheduleReplenishAmmo(playerID, hubInterface);
            }
            return response;
        }
    }
}
