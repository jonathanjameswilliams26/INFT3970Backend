using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using INFT3970Backend.Models;
using INFT3970Backend.Models.Responses;

namespace INFT3970Backend.Data_Access_Layer
{
    public class GameDAL : DataAccessLayer
    {
        /// <summary>
        /// Creates a new game using the game code passed in. Returns the create Game object. NULL if error.
        /// </summary>
        /// <param name="gameCode">The gamecode of the new game</param>
        /// <returns>Returns the created Game object. NULL data if error</returns>
        public Response<Game> CreateGame(Game game)
        {
            StoredProcedure = "usp_CreateGame";
            try
            {
                //Create the connection and command for the stored procedure
                using (Connection = new SqlConnection(ConnectionString))
                {
                    using (Command = new SqlCommand(StoredProcedure, Connection))
                    {
                        //Add the procedure input and output params
                        AddParam("gameCode", game.GameCode);
                        AddParam("timeLimit", game.TimeLimit);
                        AddParam("ammoLimit", game.AmmoLimit);
                        AddParam("startDelay", game.StartDelay);
                        AddParam("replenishAmmoDelay", game.ReplenishAmmoDelay);
                        AddParam("gameMode", game.GameMode);
                        AddParam("isJoinableAtAnytime", game.IsJoinableAtAnytime);
                        AddParam("latitude", game.Latitude);
                        AddParam("longitude", game.Longitude);
                        AddParam("radius", game.Radius);
                        AddDefaultParams();

                        //Perform the procedure and get the result
                        RunReader();
                        while (Reader.Read())
                        {
                            game = new ModelFactory(Reader).GameFactory();
                            if (game == null)
                                return new Response<Game>("An error occurred while trying to build the Game model.", ErrorCodes.BUILD_MODEL_ERROR);
                        }
                        Reader.Close();

                        //Format the results into a response object
                        ReadDefaultParams();
                        return new Response<Game>(game, ErrorMSG, Result);
                    }
                }
            }
            //A database exception was thrown, return an error response
            catch
            {
                return Response<Game>.DatabaseErrorResponse();
            }
        }



        /// <summary>
        /// Sets the game to inactive after a game was created by the host player and failed to join the game due to an unexpected
        /// error such as invalid contact details or the contact details are already taken by another player in an active playing game.
        /// </summary>
        /// <param name="gameCode">The game code being deactivated</param>
        public void DeactivateGameAfterHostJoinError(Game game)
        {
            StoredProcedure = "usp_DeactivateGameAfterHostJoinError";
            try
            {
                //Create the connection and command for the stored procedure
                using (Connection = new SqlConnection(ConnectionString))
                {
                    using (Command = new SqlCommand(StoredProcedure, Connection))
                    {
                        //Add the procedure input and output params
                        AddParam("gameID", game.GameID);
                        AddDefaultParams();
                        
                        //Perform the procedure and get the result
                        Run();
                    }
                }
            }
            //A database exception was thrown, return an error response
            catch
            {
                //Do nothing
            }
        }

        



        /// <summary>
        /// Gets the Game object matching the specified GameID
        /// </summary>
        /// <param name="gameID">The gameID of the game</param>
        /// <returns>Game object matching the specified ID. NULL if game does not exist or error occurred.</returns>
        public Response<Game> GetGameByID(int gameID)
        {
            StoredProcedure = "usp_GetGameByID";
            Game game = null;
            try
            {
                //Create the connection and command for the stored procedure
                using (Connection = new SqlConnection(ConnectionString))
                {
                    using (Command = new SqlCommand(StoredProcedure, Connection))
                    {
                        //Add the procedure input and output params
                        AddParam("gameID", gameID);
                        AddDefaultParams();

                        //Perform the procedure and get the result
                        RunReader();
                        while(Reader.Read())
                        {
                            game = new ModelFactory(Reader).GameFactory();
                            if(game == null)
                                return new Response<Game>("An error occurred while trying to build the Game model.", ErrorCodes.BUILD_MODEL_ERROR);
                        }
                        Reader.Close();

                        //Format the results into a response object
                        ReadDefaultParams();
                        return new Response<Game>(game, ErrorMSG, Result);
                    }
                }
            }
            //A database exception was thrown, return an error response
            catch
            {
                return Response<Game>.DatabaseErrorResponse();
            }
        }



        /// <summary>
        /// Creates a notification of type JOIN, sent to all players notifying.
        /// </summary>
        /// <param name="gameID">The gameID of the game</param>
        /// <param name="playerID">The playerID of who joined the game.</param>
        /// <returns>void</returns>
        public void CreateJoinNotification(Player player)
        {
            StoredProcedure = "usp_CreateJoinNotification";
            try
            {
                //Create the connection and command for the stored procedure
                using (Connection = new SqlConnection(ConnectionString))
                {
                    using (Command = new SqlCommand(StoredProcedure, Connection))
                    {
                        //Add the procedure input and output params
                        AddParam("playerID", player.PlayerID);
                        AddDefaultParams();

                        //Perform the procedure and get the result
                        Run();
                    }
                }
            }
            //A database exception was thrown, return an error response
            catch
            {
                //Do nothing
            }
        }


        /// <summary>
        /// Creates a notification of type LEAVE, sent to all players notifying.
        /// </summary>
        /// <param name="gameID">The gameID of the game</param>
        /// <param name="playerID">The playerID of who left the game.</param>
        /// <returns>void</returns>
        public void CreateLeaveNotification(Player player)
        {
            StoredProcedure = "usp_CreateLeaveNotification";
            try
            {
                //Create the connection and command for the stored procedure
                using (Connection = new SqlConnection(ConnectionString))
                {
                    using (Command = new SqlCommand(StoredProcedure, Connection))
                    {
                        //Add the procedure input and output params
                        AddParam("playerID", player.PlayerID);
                        AddDefaultParams();

                        //Perform the procedure and get the result
                        Run();
                        
                    }
                }
            }

            //A database exception was thrown, return an error response
            catch
            {
                //Do Nothing
            }
        }



        /// <summary>
        /// Creates a notification of type AMMO, sent a specific player
        /// </summary>
        /// <param name="gameID">The gameID of the game</param>
        /// <param name="playerID">The playerID of the receiver.</param>
        /// <returns>void</returns>
        public Response CreateAmmoNotification(Player player)
        {
            StoredProcedure = "usp_CreateAmmoNotification";
            try
            {
                //Create the connection and command for the stored procedure
                using (Connection = new SqlConnection(ConnectionString))
                {
                    using (Command = new SqlCommand(StoredProcedure, Connection))
                    {
                        //Add the procedure input and output params
                        AddParam("playerID", player.PlayerID);
                        AddDefaultParams();
                        
                        //Perform the procedure and get the result
                        Run();
                        
                        //Format the results into a response object
                        ReadDefaultParams();
                        return new Response(ErrorMSG, Result);
                    }
                }
            }
            //A database exception was thrown, return an error response
            catch
            {
                return Response.DatabaseErrorResponse();
            }
        }



        /// <summary>
        /// Creates a notification of type SUCCESS or FAIL, sent to specific player.
        /// </summary>
        /// <param name="gameID">The gameID of the game</param>
        /// <param name="playerID">The playerID of the receiver.</param>
        /// <returns>void</returns>
        public void CreateTagResultNotification(Photo photo)
        {
            StoredProcedure = "usp_CreateTagResultNotification";
            try
            {
                //Create the connection and command for the stored procedure
                using (Connection = new SqlConnection(ConnectionString))
                {
                    using (Command = new SqlCommand(StoredProcedure, Connection))
                    {
                        //Add the procedure input and output params
                        AddParam("takenByID", photo.TakenByPlayerID);
                        AddParam("photoOfID", photo.PhotoOfPlayerID);
                        AddParam("decision", photo.IsSuccessful);
                        AddDefaultParams();

                        //Perform the procedure and get the result
                        Run();
                        ReadDefaultParams();
                    }
                }
            }

            //A database exception was thrown, return an error response
            catch
            {
                //Do nothing
            }
        }







        /// <summary>
        /// Completes A Game in the Database.
        /// </summary>
        /// <param name="gameID">The ID of the Game to Complete</param>
        /// <returns></returns>
        public Response CompleteGame(int gameID)
        {
            StoredProcedure = "usp_CompleteGame";
            try
            {
                //Create the connection and command for the stored procedure
                using (Connection = new SqlConnection(ConnectionString))
                {
                    using (Command = new SqlCommand(StoredProcedure, Connection))
                    {
                        //Add the procedure input and output params
                        AddParam("gameID", gameID);
                        AddDefaultParams();

                        //Perform the procedure and get the result
                        Run();

                        //Build the response object
                        ReadDefaultParams();
                        return new Response(ErrorMSG, Result);
                    }
                }
            }
            //A database exception was thrown, return an error response
            catch
            {
                return Response.DatabaseErrorResponse();
            }
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
        public Response<Game> GetAllPlayersInGame(int id, bool isPlayerID, string filter, string orderBy)
        {
            StoredProcedure = "usp_GetAllPlayersInGame";
            List<Player> players = new List<Player>();
            try
            {
                //Create the connection and command for the stored procedure
                using (Connection = new SqlConnection(ConnectionString))
                {
                    using (Command = new SqlCommand(StoredProcedure, Connection))
                    {
                        //Add the procedure input and output params
                        AddParam("id", id);
                        AddParam("isPlayerID", isPlayerID);
                        AddParam("filter", filter);
                        AddParam("orderBy", orderBy);
                        AddDefaultParams();

                        //Perform the procedure and get the result
                        RunReader();

                        //read the player list, if an error occurred in the stored procedure there will be no results to read an this will be skipped
                        ModelFactory factory = new ModelFactory(Reader);
                        while (Reader.Read())
                        {
                            //Call the ModelFactory to build the model from the data
                            Player player = factory.PlayerFactory(false);

                            //If an error occurred while trying to build the player list
                            if (player == null)
                                return new Response<Game>("An error occurred while trying to build the player list.", ErrorCodes.BUILD_MODEL_ERROR);

                            players.Add(player);
                        }
                        Reader.Close();
                        
                        ReadDefaultParams();

                        //If the response was not successful return the empty response
                        if(IsError)
                            return new Response<Game>(ErrorMSG, Result);
                    }
                }

                //If the players list is empty, return an error
                if(players.Count == 0)
                    return new Response<Game>("The players list is empty.", ErrorCodes.ITEM_DOES_NOT_EXIST);

                //Get the Game object
                Response<Game> gameResponse = GetGameByID(players[0].GameID);

                //If the Game response was not successful, return an error
                if(!gameResponse.IsSuccessful())
                    return new Response<Game>(gameResponse.ErrorMessage, gameResponse.ErrorCode);

                //Otherwise, create the successful response with the game and player list
                gameResponse.Data.Players = players;
                return new Response<Game>(gameResponse.Data, ErrorMSG, Result);
            }

            //A database exception was thrown, return an error response
            catch
            {
                return Response<Game>.DatabaseErrorResponse();
            }
        }







        /// <summary>
        /// Sets the Game State to starting in the database, updates the game start time to 10mins
        /// in the future and sets the Game end time. After this method is run code will be scheduled
        /// in the business logic to update the game to playing, then also update the game to be completed
        /// </summary>
        /// <param name="playerID">The ID of the host player of the game. Only the host player can begin the game.</param>
        /// <returns>The updated game object, NULL if an error occurred</returns>
        public Response<Game> BeginGame(Player player)
        {
            StoredProcedure = "usp_BeginGame";
            Game game = null;
            try
            {
                //Create the connection and command for the stored procedure
                using (Connection = new SqlConnection(ConnectionString))
                {
                    using (Command = new SqlCommand(StoredProcedure, Connection))
                    {
                        //Add the procedure input and output params
                        AddParam("playerID", player.PlayerID);
                        AddDefaultParams();

                        //Perform the procedure and get the result
                        RunReader();
                        while (Reader.Read())
                        {
                            game = new ModelFactory(Reader).GameFactory();
                            if(game == null)
                                return new Response<Game>("An error occurred while trying to build the Game model.", ErrorCodes.BUILD_MODEL_ERROR);
                        }
                        Reader.Close();

                        //Format the results into a response object
                        ReadDefaultParams();
                        return new Response<Game>(game, ErrorMSG, Result);
                    }
                }
            }
            //A database exception was thrown, return an error response
            catch
            {
                return Response<Game>.DatabaseErrorResponse();
            }
        }




        /// <summary>
        /// Sets the GameState to the state passed in.
        /// </summary>
        /// <param name="gameID">The ID of the game to update</param>
        /// <param name="gameState">The new state to set.</param>
        /// <returns></returns>
        public Response SetGameState(int gameID, string gameState)
        {
            StoredProcedure = "usp_SetGameState";
            try
            {
                //Create the connection and command for the stored procedure
                using (Connection = new SqlConnection(ConnectionString))
                {
                    using (Command = new SqlCommand(StoredProcedure, Connection))
                    {
                        //Add the procedure input and output params
                        AddParam("gameID", gameID);
                        AddParam("gameState", gameState);
                        AddDefaultParams();

                        //Perform the procedure and get the result
                        Run();

                        //Format the results into a response object
                        ReadDefaultParams();
                        return new Response(ErrorMSG, Result);
                    }
                }
            }
            //A database exception was thrown, return an error response
            catch
            {
                return Response.DatabaseErrorResponse();
            }
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
        public Response<GameStatusResponse> GetGameStatus(Player player)
        {
            StoredProcedure = "usp_GetGameStatus";
            Response<GameStatusResponse> response = null;
            try
            {
                //Create the connection and command for the stored procedure
                using (Connection = new SqlConnection(ConnectionString))
                {
                    using (Command = new SqlCommand(StoredProcedure, Connection))
                    {
                        //Add the procedure input and output params
                        AddParam("playerID", player.PlayerID);
                        AddOutput("gameState", SqlDbType.VarChar);
                        AddOutput("hasVotesToComplete", SqlDbType.Bit);
                        AddOutput("hasNotifications", SqlDbType.Bit);
                        AddDefaultParams();

                        //Perform the procedure and get the result
                        RunReader();
                        Player updatedPlayer = null;
                        while(Reader.Read())
                        {
                            updatedPlayer = new ModelFactory(Reader).PlayerFactory(true);
                            if(player == null)
                                return new Response<GameStatusResponse>("An error occurred while trying to build the Player model.", ErrorCodes.BUILD_MODEL_ERROR);
                        }
                        Reader.Close();

                        //Format the results into a response object
                        ReadDefaultParams();
                        response = new Response<GameStatusResponse>(null, ErrorMSG, Result);

                        //If the result is not an error build the GameStatus object and assign it to the response
                        if(!IsError)
                        {
                            string gameState = Convert.ToString(Command.Parameters["@gameState"].Value);
                            bool hasVotesToComplete = Convert.ToBoolean(Command.Parameters["@hasVotesToComplete"].Value);
                            bool hasNotifications = Convert.ToBoolean(Command.Parameters["@hasNotifications"].Value);
                            GameStatusResponse gsr = new GameStatusResponse(gameState, hasVotesToComplete, hasNotifications, updatedPlayer);
                            response.Data = gsr;
                        }
                        return response;
                    }
                }
            }
            //A database exception was thrown, return an error response
            catch
            {
                return Response<GameStatusResponse>.DatabaseErrorResponse();
            }
        }
    }
}
