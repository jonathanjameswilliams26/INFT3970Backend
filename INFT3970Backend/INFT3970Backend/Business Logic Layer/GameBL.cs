using INFT3970Backend.Data_Access_Layer;
using INFT3970Backend.Hubs;
using INFT3970Backend.Models;
using INFT3970Backend.Models.Responses;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading;

namespace INFT3970Backend.Business_Logic_Layer
{
    public class GameBL
    {
        /// <summary>
        /// Creates a new game with a randomly generated lobby code.
        /// Returns the created Game object. NULL data if successful.
        /// </summary>
        /// <returns>Returns the created Game object. NULL data if successful.</returns>
        public Response<Player> CreateGame(string nickname, string contact, string imgUrl)
        {
            GameDAL gameDAL = new GameDAL();
            Response<Game> response = null;
            Response<Player> createdPlayer = null;

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


            //If the create game failed, return the error message and code from that response
            if (!response.IsSuccessful())
                return new Response<Player>(null, "ERROR", response.ErrorMessage, response.ErrorCode);

            //Call the Player Business Logic to join the player to the game
            PlayerBL playerBL = new PlayerBL();
            createdPlayer = playerBL.JoinGame(response.Data.GameCode, nickname, contact, imgUrl, true);

            //If the Host player failed to join the game deleted the created game
            if (!createdPlayer.IsSuccessful())
            {
                DeactivateGameAfterHostJoinError(response.Data.GameID);
                return new Response<Player>(null, "ERROR", createdPlayer.ErrorMessage, createdPlayer.ErrorCode);
            }

            return createdPlayer;
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
        public Response<GamePlayerListResponse> GetAllPlayersInGame(int id, bool isPlayerID, string filter, string orderBy)
        {
            //Validate the filer and orderby value is a valid value
            if (string.IsNullOrWhiteSpace(filter) || string.IsNullOrWhiteSpace(orderBy))
                return new Response<GamePlayerListResponse>(null, "ERROR", "The filter or orderBy value is null or empty.", ErrorCodes.EC_DATAINVALID);

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






        /// <summary>
        /// Begins the game.
        /// </summary>
        /// <param name="playerID">The ID of the host player beginning the game, only the host player can begin the game.</param>
        /// <param name="hubContext">The hub context used to send live updates to clients</param>
        /// <returns>The updated Game object after being updated in the database.</returns>
        public Response<Game> BeginGame(int playerID, IHubContext<ApplicationHub> hubContext)
        {
            //Call the data access layer to begin the game, set the Game to a STARTING state
            GameDAL gameDAL = new GameDAL();
            Response<Game> response = gameDAL.BeginGame(playerID);

            if(response.IsSuccessful())
            {
                HubInterface hubInterface = new HubInterface(hubContext);
                ScheduledTasks.ScheduleGameInPlayingState(response.Data, hubInterface);
                ScheduledTasks.ScheduleCompleteGame(response.Data, hubInterface);

                //Update all clients that the game is now in a starting state and the game will be playing soon
                hubInterface.UpdateGameInStartingState(response.Data);
            }
            return response;
        }






        /// <summary>
        /// Get the current status of the game / web application. Used when a user reconnects
        /// back to the web application in order to the front end to be updated with the current
        /// game / application state so the front end can redirect the user accordingly.
        /// </summary>
        /// <param name="playerID">The ID of the player</param>
        /// <returns>
        /// A GameStatusResponse object which outlines the GameState, 
        /// if the player has votes to complete, if the player has any new notifications 
        /// and the most recent player record. NULL if an error occurred.
        /// </returns>
        public Response<GameStatusResponse> GetGameStatus(int playerID)
        {
            //Call the data access layer to get the status of the game / player
            GameDAL gameDAL = new GameDAL();
            return gameDAL.GetGameStatus(playerID);
        }
    }
}
