using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using INFT3970Backend.Data_Access_Layer;
using INFT3970Backend.Models;
using INFT3970Backend.Hubs;

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
        /// <returns></returns>
        public Response<int> JoinGame(string gameCode, string nickname, string contact)
        {
            //Confirm the game code is 6 characters in length and only contains letters and numbers
            Regex gameCodeRegex = new Regex(@"^[a-zA-Z0-9]{6,6}$");
            if (!gameCodeRegex.IsMatch(gameCode))
                return new Response<int>(-1, ResponseType.ERROR, "The game code is incorrect, it must be 6 characters long and only contain letters and numbers.");

            //Confirm the nickname is not empty
            if (String.IsNullOrEmpty(nickname))
                return new Response<int>(-1, ResponseType.ERROR, "You must enter a nickname, your nickname cannot be blank.");

            //Confirm the nickname is only numbers and letters (no spaces allowed)
            Regex nicknameRegex = new Regex(@"^[a-zA-Z0-9]{1,}$");
            bool march = nicknameRegex.IsMatch(nickname);
            if (!nicknameRegex.IsMatch(nickname))
                return new Response<int>(-1, ResponseType.ERROR, "The nickname you entered is invalid, please only enter letters and numbers (no spaces).");

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
                return new Response<int>(-1, ResponseType.ERROR, "The contact information entered is invalid. Please enter a phone number or an email address.");

            
            //If the contact is a phone number, reformat the number to use +61 since that is needed for twilio
            if(isPhone)
                contact = "+61" + contact.Substring(1);


            //Generate a verification code for the player to verify their contact details (5 digit verification code)
            Random rand = new Random();
            int verificationCode = rand.Next(10000, 99999);


            //Call the data access layer to add the player to the database
            PlayerDAL playerDAL = new PlayerDAL();
            Response<int> response = playerDAL.JoinGame(gameCode, nickname, contact, isPhone, verificationCode);

            //If the response was successful, send the verification code to the player
            if(response.Type == ResponseType.SUCCESS)
            {
                bool didSend = false;
                //if (isPhone)
                    //didSend = new TextMessageSender(contact).SendVerificationCode(verificationCode);
                //else
                    didSend = new EmailSender().SendVerificationEmail(verificationCode, contact);

                //If the message did not send correctly then update the response to now be an error
                if (!didSend)
                {
                    response.Type = ResponseType.ERROR;
                    response.ErrorMessage = "An error occurred while trying to send your verification code.";
                }
            }

            return response;
        }
    }
}
