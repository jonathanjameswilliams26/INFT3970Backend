using INFT3970Backend.Data_Access_Layer;
using INFT3970Backend.Hubs;
using INFT3970Backend.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;

namespace INFT3970Backend.Business_Logic_Layer
{
    public class GameBL
    {
        /// <summary>
        /// Creates a new game with a randomly generated lobby code.
        /// Returns the created Game object. NULL data if successful.
        /// </summary>
        /// <returns>Returns the created Game object. NULL data if successful.</returns>
        public Response<Game> CreateGame()
        {
            GameDAL gameDAL = new GameDAL();
            Response<Game> response = null;

            bool doRun = true;
            while (doRun)
            {
                //Call the data access layer to create a new game using the code passed in
                response = gameDAL.CreateGame(GenerateCode());

                //If the response is successful the game was successfully created
                if (response.Type == "ERROR")
                    doRun = false;

                //If the response contains an error code of ITEMALREADYEXISTS then the game code is not unique,
                //Want to run again and generate a new code
                else if (response.ErrorCode == ErrorCodes.EC_ITEMALREADYEXISTS)
                    doRun = true;

                //If the response contains any other error code we do not want to run again because an un expected
                //error occurred and we want to return the error response.
                else
                    doRun = false;
            }
            return response;
        }




        /// <summary>
        /// Deactivates the game created by the host player because an unexpected error occurred while the host player tried to join the game
        /// </summary>
        /// <param name="gameCode">The game code to deactivate</param>
        public void DeactivateGameAfterHostJoinError(int gameID)
        {
            new GameDAL().DeactivateGameAfterHostJoinError(gameID);
        }

        

       
        /// <summary>
        /// Generates the game code for the game which players use to join the game.
        /// </summary>
        /// <returns>The randomly generated game code</returns>
        private string GenerateCode()
        {
            //GENERATE CODE
            char[] chars = "abcdefghijklmnopqrstuvwxyz1234567890".ToCharArray();
            string gameCode = string.Empty;
            Random random = new Random();

            for (int i = 0; i < 6; i++)
            {
                int x = random.Next(0, chars.Length);
                //For avoiding repetition of Characters
                if (!gameCode.Contains(chars.GetValue(x).ToString()))
                    gameCode += chars.GetValue(x);
                else
                    i = i - 1;
            }

            return gameCode;
        }



        /// <summary>
        /// Creates a notification for a player joining a game. Sent to all.
        /// </summary>
        /// <returns>void</returns>
        public void CreateJoinNotification(int playerID)
        {
            GameDAL gameDAL = new GameDAL();
            gameDAL.CreateJoinNotification(playerID);
        }

        /// <summary>
        /// Creates a notification for a player leaving a game. Sent to all.
        /// </summary>
        /// <returns>void</returns>
        public void CreateLeaveNotification(int playerID)
        {
            GameDAL gameDAL = new GameDAL();
            gameDAL.CreateLeaveNotification(playerID);
        }

        /// <summary>
        /// Creates a notification for a player that their ammo is refilled.
        /// </summary>
        /// <returns>void</returns>
        public Response<object> CreateAmmoNotification(int playerID)
        {
            GameDAL gameDAL = new GameDAL();
            return gameDAL.CreateAmmoNotification(playerID);
        }


        /// <summary>
        /// Creates a notification for everyone regarding the result of a photo tag.
        /// </summary>
        /// <param name="takenByID">The ID of the player who took the photo</param>
        /// <param name="photoOfID">The ID of the player who the photo is off</param>
        /// <param name="decision">The successful or unsuccessful decision of the photo</param>
        public void CreateTagResultNotification(int takenByID, int photoOfID, bool decision)
        {
            GameDAL gameDAL = new GameDAL();
            gameDAL.CreateTagResultNotification(takenByID, photoOfID, decision);
        }




        /// <summary>
        /// Gets the Game object matching the specified ID.
        /// </summary>
        /// <param name="gameID">The gameID to return.</param>
        /// <returns>A Game object if the ID exists, NULL if the ID does not exist.</returns>
        public Response<Game> GetGame(int gameID)
        {
            GameDAL gameDAL = new GameDAL();
            return gameDAL.GetGameByID(gameID);
        }



        /// <summary>
        /// Completes a game of CamTag in the Database.
        /// </summary>
        /// <param name="gameID">The GameID to complete</param>
        /// <param name="hubContext">The hub context used to send notifications to users.</param>
        /// <returns></returns>
        public Response<object> CompleteGame(int gameID, IHubContext<ApplicationHub> hubContext)
        {
            //Call the DataAccessLayer to complete the game in the DB
            GameDAL gameDAL = new GameDAL();
            Response<object> response = gameDAL.CompleteGame(gameID);

            //If the response was successful send out the game completed messages to players
            if(response.IsSuccessful())
            {
                HubInterface hubInterface = new HubInterface(hubContext);
                hubInterface.UpdateGameCompleted(gameID);
            }

            return response;
        }








        /// <summary>
        /// Gets all the players in a game with multiple filter parameters
        /// 
        /// FILTER
        /// ALL = get all the players in the game which arnt deleted
        /// ACTIVE = get all players in the game which arnt deleted and is active
        /// INGAME = get all players in the game which arnt deleted, is active, have not left the game and have been verified
        /// INGAMEALL = get all players in the game which arnt deleted, is active, and have been verified(includes players who have left the game)
        ///
        /// ORDER by
        /// AZ = Order by name in alphabetical order
        /// ZA = Order by name in reverse alphabetical order
        /// KILLS= Order from highest to lowest in number of kills
        /// </summary>
        /// <param name="id">The playerID or the GameID</param>
        /// <param name="isPlayerID">A flag which outlines if the ID passed in is a playerID</param>
        /// <param name="filter">The filter value, ALL, ACTIVE, INGAME, INGAMEALL</param>
        /// <param name="orderBy">The order by value, AZ, ZA, KILLS</param>
        /// <returns>The list of all players in the game</returns>
        public Response<GamePlayerList> GetAllPlayersInGame(int id, bool isPlayerID, string filter, string orderBy)
        {
            //Validate the filer and orderby value is a valid value
            if (string.IsNullOrWhiteSpace(filter) || string.IsNullOrWhiteSpace(orderBy))
                return new Response<GamePlayerList>(null, "ERROR", "The filter or orderBy value is null or empty.", ErrorCodes.EC_DATAINVALID);

            //Confirm the filter value passed in is a valid value
            bool isFilterValid = false;
            if (filter.ToUpper() == "ALL" || filter.ToUpper() == "ACTIVE" || filter.ToUpper() == "INGAME" || filter.ToUpper() == "INGAMEALL")
                isFilterValid = true;


            //Confirm the order by value passed in is a valid value
            bool isOrderByValid = false;
            if (orderBy.ToUpper() == "AZ" || orderBy.ToUpper() == "ZA" || orderBy.ToUpper() == "KILLS")
                isOrderByValid = true;


            //If any of the values are invalid, set them to a default value
            if (!isFilterValid)
                filter = "ALL";
            if (!isOrderByValid)
                orderBy = "AZ";

            //Call the data access layer to get the list
            GameDAL gameDAL = new GameDAL();
            return gameDAL.GetAllPlayersInGame(id, isPlayerID, filter, orderBy);
        }
    }
}
