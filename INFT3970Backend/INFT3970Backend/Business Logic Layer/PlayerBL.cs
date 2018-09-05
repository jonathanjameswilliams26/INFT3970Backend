using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using INFT3970Backend.Data_Access_Layer;
using INFT3970Backend.Models;
using INFT3970Backend.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace INFT3970Backend.Business_Logic_Layer
{
    public class PlayerBL
    {
        /// <summary>
        /// Get the list of all players currently inside a game. RESPONSE DATA = List of Player objects, or NULL if error
        /// </summary>
        /// <param name="playerID">The player ID. This playerID can be used to find what game they are in and get all other players</param>
        /// <returns>A list of all the players currently inside the game which the passed in playerID is in.</returns>
        public Response<List<Player>> GetAllPlayersInGame(int playerID)
        {
            //Call the DataAccessLayer to get the list of players in the same game from the database
            PlayerDAL playerDAL = new PlayerDAL();
            return playerDAL.GetGamePlayerList(playerID);
        }






        /// <summary>
        /// Updates the players connection ID. RESPONSE DATA = NULL
        /// </summary>
        /// <param name="playerID">The PlayerID being updated</param>
        /// <param name="connectionID">The new ConnectionID</param>
        /// <returns></returns>
        public Response<object> UpdateConnectionID(int playerID, string connectionID)
        {
            //Call the Data Access Layer to update the playerID's connectionID in the Database
            PlayerDAL playerDAL = new PlayerDAL();
            return playerDAL.UpdateConnectionID(playerID, connectionID);
        }







        /// <summary>
        /// Joins a new player to a game matching the game code passed in. Stores their player details and sends them a verification code once they have joined the game.
        /// RESPONSE DATA = The PlayerID created in the DB, or negative INT if error.
        /// </summary>
        /// <param name="gameCode">The game the player is trying to join.</param>
        /// <param name="nickname">The players nickname in the game</param>
        /// <param name="contact">The players contact either phone or email where the player will be contacted throughout the game.</param>
        /// <param name="isHost">A flag which outlines if the player joining the game is the host of the game.</param>
        /// <returns></returns>
        public Response<int> JoinGame(string gameCode, string nickname, string contact, bool isHost)
        {
            //Confirm the input parameters are not empty or null
            if (String.IsNullOrWhiteSpace(nickname) || String.IsNullOrWhiteSpace(gameCode) || String.IsNullOrWhiteSpace(contact))
                return new Response<int>(-1, ResponseType.ERROR, "Missing request data, nickname, contact or gamecode is empty or null.", ErrorCodes.EC_MISSINGORBLANKDATA);

            //Confirm the game code is 6 characters in length and only contains letters and numbers
            Regex gameCodeRegex = new Regex(@"^[a-zA-Z0-9]{6,6}$");
            if (!gameCodeRegex.IsMatch(gameCode))
                return new Response<int>(-1, ResponseType.ERROR, "The game code is incorrect, it must be 6 characters long and only contain letters and numbers.", ErrorCodes.EC_JOINGAME_INVALIDGAMECODE);

            //Confirm the nickname is only numbers and letters (no spaces allowed)
            Regex nicknameRegex = new Regex(@"^[a-zA-Z0-9]{1,}$");
            bool march = nicknameRegex.IsMatch(nickname);
            if (!nicknameRegex.IsMatch(nickname))
                return new Response<int>(-1, ResponseType.ERROR, "The nickname you entered is invalid, please only enter letters and numbers (no spaces).", ErrorCodes.EC_JOINGAME_NICKNAMEINVALID);

            //Confirm the contact, check if it is an email or phone number
            bool isPhone = false;
            bool isEmail = false;

            //Check to see if the contact is a phone number
            Regex phoneRegex = new Regex(@"^[0-9]{10,10}$");
            isPhone = phoneRegex.IsMatch(contact);

            //Check to see if the contact is an email address
            //REFERENCE: http://emailregex.com/
            Regex emailRegex = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
            isEmail = emailRegex.IsMatch(contact);

            //If the contact is not either a phone or email address return an error
            if (!isEmail && !isPhone)
                return new Response<int>(-1, ResponseType.ERROR, "The contact information entered is invalid. Please enter a phone number or an email address.", ErrorCodes.EC_JOINGAME_CONTACTINVALID);

            
            //If the contact is a phone number, reformat the number to use +61 since that is needed for twilio
            if(isPhone)
                contact = "+61" + contact.Substring(1);

            //Call the data access layer to add the player to the database
            int verificationCode = GenerateVerificationCode();
            PlayerDAL playerDAL = new PlayerDAL();
            Response<int> response = playerDAL.JoinGame(gameCode, nickname, contact, isPhone, verificationCode, isHost);

            //If the response was successful, send the verification code to the player
            if(response.Type == ResponseType.SUCCESS)
            {
                bool didSend = SendVerificationCode(verificationCode, contact, isPhone);

                //If the message did not send correctly then update the response to now be an error
                if (!didSend)
                {
                    response.Type = ResponseType.ERROR;
                    response.ErrorMessage = "An error occurred while trying to send your verification code.";
                    response.ErrorCode = ErrorCodes.EC_VERIFICATIONCODESENDERROR;
                }
            }
            return response;
        }







        /// <summary>
        /// Validates a players received verification code. If the player successfully enters their verification code
        /// their player record will be verified, meaning they have confirmed their email address or phone number is correct
        /// and has access to it throughout the game.
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
                return new Response<object>(null, ResponseType.ERROR, "Missing request data, verification code is empty or null or playerID was not valid.", ErrorCodes.EC_MISSINGORBLANKDATA);


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
                return new Response<object>(null, ResponseType.ERROR, "The verification code is invalid. Must be an INT between 10000 and 99999.", ErrorCodes.EC_VERIFYPLAYER_CODEINVALID);
            }

            //Call the data access layer to confirm the verification code is correct.
            PlayerDAL playerDAL = new PlayerDAL();
            Response<object> response = playerDAL.ValidateVerificationCode(code, playerID);

            //If the player was successfully verified, updated all the clients about a joined player.
            if(response.Type == ResponseType.SUCCESS)
            {
                HubInterface hubInterface = new HubInterface(hubContext);
                hubInterface.UpdatePlayerJoined(playerID);
            }

            return response;
        }










        /// <summary>
        /// Regenerates a verification code, updates the player record to store the new code and sends the new code to the
        /// players contact (Email or Phone). Returns a response with NULL data, use Response.Type to determine error or success.
        /// </summary>
        /// <param name="playerID">The player who's verification code is being updated.</param>
        /// <returns></returns>
        public Response<object> ResendVerificationCode(int playerID)
        {
            //Generate a new verification code to send
            int code = GenerateVerificationCode();

            //Call the Data Access Layer to update the verification code for the player and get the contact information for the player
            Response<string> response = new PlayerDAL().UpdateVerificationCode(playerID, code);

            //If the response is successful send the verification code
            bool didSend = false;
            if (response.Type == ResponseType.SUCCESS)
            {
                //The contact information returned from the data access is an email address
                if (response.Data.Contains("@"))
                    didSend = SendVerificationCode(code, response.Data, false);

                //Otherwise, the contact information returned is a phone number
                else
                    didSend = SendVerificationCode(code, response.Data, true);
            }
            //Otherwise, an error occurred while updating the validation code, return the error obtained from the database
            else
                return new Response<object>(null, ResponseType.ERROR, response.ErrorMessage, response.ErrorCode);


            //Confirm the verification code sent, return a success response or error message
            if (didSend)
                return new Response<object>(null, ResponseType.SUCCESS, null, 1);
            else
                return new Response<object>(null, ResponseType.ERROR, "An error occurred while trying to resend the verification code.", ErrorCodes.EC_VERIFICATIONCODESENDERROR);
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
        /// Sends a verification code to the email address or phone number. Returns TRUE if sent successfully, FALSE otherwise
        /// </summary>
        /// <param name="code">The verification code to send</param>
        /// <param name="sendTo">The email or phone number to send to</param>
        /// <param name="isPhone">A flag outlining if the value is a phone number</param>
        /// <returns></returns>
        private bool SendVerificationCode(int code, string sendTo, bool isPhone)
        {
            if (isPhone)
                return new TextMessageSender(sendTo).SendVerificationCode(code);
            else
                return new EmailSender().SendVerificationEmail(code, sendTo);
        }
    }
}
