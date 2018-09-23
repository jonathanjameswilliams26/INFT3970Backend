using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
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
        public Response<Game> CreateGame(string gameCode)
        {
            StoredProcedure = "usp_CreateGame";
            Game game = null;
            try
            {
                //Create the connection and command for the stored procedure
                using (Connection = new SqlConnection(ConnectionString))
                {
                    using (Command = new SqlCommand(StoredProcedure, Connection))
                    {
                        //Add the procedure input and output params
                        Command.CommandType = CommandType.StoredProcedure;
                        Command.Parameters.AddWithValue("@gameCode", gameCode);
                        Command.Parameters.Add("@result", SqlDbType.Int);
                        Command.Parameters["@result"].Direction = ParameterDirection.Output;
                        Command.Parameters.Add("@errorMSG", SqlDbType.VarChar, 255);
                        Command.Parameters["@errorMSG"].Direction = ParameterDirection.Output;

                        //Perform the procedure and get the result
                        Connection.Open();
                        Reader = Command.ExecuteReader();
                        while (Reader.Read())
                        {
                            game = new ModelFactory(Reader).GameFactory();
                            if (game == null)
                                return new Response<Game>(null, "ERROR", "An error occurred while trying to build the Game model.", ErrorCodes.EC_BUILDMODELERROR);
                        }
                        Reader.Close();

                        //Format the results into a response object
                        Result = Convert.ToInt32(Command.Parameters["@result"].Value);
                        ErrorMSG = Convert.ToString(Command.Parameters["@errorMSG"].Value);
                        return new Response<Game>(game, Result, ErrorMSG, Result);
                    }
                }
            }

            //A database exception was thrown, return an error response
            catch
            {
                return new Response<Game>(null, "ERROR", DatabaseErrorMSG, ErrorCodes.EC_DATABASECONNECTERROR);
            }
        }



        /// <summary>
        /// Sets the game to inactive after a game was created by the host player and failed to join the game due to an unexpected
        /// error such as invalid contact details or the contact details are already taken by another player in an active playing game.
        /// </summary>
        /// <param name="gameCode">The game code being deactivated</param>
        public void DeactivateGameAfterHostJoinError(int gameID)
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
                        Command.CommandType = CommandType.StoredProcedure;
                        Command.Parameters.AddWithValue("@gameID", gameID);
                        Command.Parameters.Add("@result", SqlDbType.Int);
                        Command.Parameters["@result"].Direction = ParameterDirection.Output;
                        Command.Parameters.Add("@errorMSG", SqlDbType.VarChar, 255);
                        Command.Parameters["@errorMSG"].Direction = ParameterDirection.Output;

                        //Perform the procedure and get the result
                        Connection.Open();
                        Command.ExecuteNonQuery();
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
                        Command.CommandType = CommandType.StoredProcedure;
                        Command.Parameters.AddWithValue("@gameID", gameID);
                        Command.Parameters.Add("@result", SqlDbType.Int);
                        Command.Parameters["@result"].Direction = ParameterDirection.Output;
                        Command.Parameters.Add("@errorMSG", SqlDbType.VarChar, 255);
                        Command.Parameters["@errorMSG"].Direction = ParameterDirection.Output;

                        //Perform the procedure and get the result
                        Connection.Open();
                        Reader = Command.ExecuteReader();
                        while(Reader.Read())
                        {
                            game = new ModelFactory(Reader).GameFactory();
                            if(game == null)
                                return new Response<Game>(null, "ERROR", "An error occurred while trying to build the Game model.", ErrorCodes.EC_BUILDMODELERROR);
                        }
                        Reader.Close();

                        //Format the results into a response object
                        Result = Convert.ToInt32(Command.Parameters["@result"].Value);
                        ErrorMSG = Convert.ToString(Command.Parameters["@errorMSG"].Value);
                        return new Response<Game>(game, Result, ErrorMSG, Result);
                    }
                }
            }

            //A database exception was thrown, return an error response
            catch
            {
                return new Response<Game>(null, "ERROR", DatabaseErrorMSG, ErrorCodes.EC_DATABASECONNECTERROR);
            }
        }



        /// <summary>
        /// Creates a notification of type JOIN, sent to all players notifying.
        /// </summary>
        /// <param name="gameID">The gameID of the game</param>
        /// <param name="playerID">The playerID of who joined the game.</param>
        /// <returns>void</returns>
        public void CreateJoinNotification(int playerID)
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
                        Command.CommandType = CommandType.StoredProcedure;
                        Command.Parameters.AddWithValue("@playerID", playerID);
                        Command.Parameters.Add("@result", SqlDbType.Int);
                        Command.Parameters["@result"].Direction = ParameterDirection.Output;
                        Command.Parameters.Add("@errorMSG", SqlDbType.VarChar, 255);
                        Command.Parameters["@errorMSG"].Direction = ParameterDirection.Output;

                        //Perform the procedure and get the result
                        Connection.Open();
                        Command.ExecuteNonQuery();
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
        public void CreateLeaveNotification(int playerID)
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
                        Command.CommandType = CommandType.StoredProcedure;
                        Command.Parameters.AddWithValue("@playerID", playerID);
                        Command.Parameters.Add("@result", SqlDbType.Int);
                        Command.Parameters["@result"].Direction = ParameterDirection.Output;
                        Command.Parameters.Add("@errorMSG", SqlDbType.VarChar, 255);
                        Command.Parameters["@errorMSG"].Direction = ParameterDirection.Output;

                        //Perform the procedure and get the result
                        Connection.Open();
                        Command.ExecuteNonQuery();
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
        public Response<object> CreateAmmoNotification(int playerID)
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
                        Command.CommandType = CommandType.StoredProcedure;
                        Command.Parameters.AddWithValue("@playerID", playerID);
                        Command.Parameters.Add("@result", SqlDbType.Int);
                        Command.Parameters["@result"].Direction = ParameterDirection.Output;
                        Command.Parameters.Add("@errorMSG", SqlDbType.VarChar, 255);
                        Command.Parameters["@errorMSG"].Direction = ParameterDirection.Output;

                        //Perform the procedure and get the result
                        Connection.Open();
                        Command.ExecuteNonQuery();

                        //Format the results into a response object
                        Result = Convert.ToInt32(Command.Parameters["@result"].Value);
                        ErrorMSG = Convert.ToString(Command.Parameters["@errorMSG"].Value);
                        return new Response<object>(null, Result, ErrorMSG, Result);
                    }
                }
            }

            //A database exception was thrown, return an error response
            catch
            {
                return new Response<object>(null, "ERROR", DatabaseErrorMSG, ErrorCodes.EC_DATABASECONNECTERROR);
            }
        }



        /// <summary>
        /// Creates a notification of type SUCCESS or FAIL, sent to specific player.
        /// </summary>
        /// <param name="gameID">The gameID of the game</param>
        /// <param name="playerID">The playerID of the receiver.</param>
        /// <returns>void</returns>
        public void CreateTagResultNotification(int takenByID, int photoOfID, bool decision)
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
                        Command.CommandType = CommandType.StoredProcedure;
                        Command.Parameters.AddWithValue("@takenByID", takenByID);
                        Command.Parameters.AddWithValue("@photoOfID", photoOfID);
                        Command.Parameters.AddWithValue("@decision", decision);
                        Command.Parameters.Add("@result", SqlDbType.Int);
                        Command.Parameters["@result"].Direction = ParameterDirection.Output;
                        Command.Parameters.Add("@errorMSG", SqlDbType.VarChar, 255);
                        Command.Parameters["@errorMSG"].Direction = ParameterDirection.Output;

                        //Perform the procedure and get the result
                        Connection.Open();
                        Command.ExecuteNonQuery();


                        Result = Convert.ToInt32(Command.Parameters["@result"].Value);
                        ErrorMSG = Convert.ToString(Command.Parameters["@errorMSG"].Value);
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
        public Response<object> CompleteGame(int gameID)
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
                        Command.CommandType = CommandType.StoredProcedure;
                        Command.Parameters.AddWithValue("@gameID", gameID);
                        Command.Parameters.Add("@result", SqlDbType.Int);
                        Command.Parameters["@result"].Direction = ParameterDirection.Output;
                        Command.Parameters.Add("@errorMSG", SqlDbType.VarChar, 255);
                        Command.Parameters["@errorMSG"].Direction = ParameterDirection.Output;

                        //Perform the procedure and get the result
                        Connection.Open();
                        Command.ExecuteNonQuery();

                        //Build the response object
                        Result = Convert.ToInt32(Command.Parameters["@result"].Value);
                        ErrorMSG = Convert.ToString(Command.Parameters["@errorMSG"].Value);
                        return new Response<object>(null, Result, ErrorMSG, Result);
                    }
                }
            }

            //A database exception was thrown, return an error response
            catch
            {
                return new Response<object>(null, "ERROR", DatabaseErrorMSG, ErrorCodes.EC_DATABASECONNECTERROR);
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
        public Response<GamePlayerListResponse> GetAllPlayersInGame(int id, bool isPlayerID, string filter, string orderBy)
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
                        Command.CommandType = CommandType.StoredProcedure;
                        Command.Parameters.AddWithValue("@id", id);
                        Command.Parameters.AddWithValue("@isPlayerID", isPlayerID);
                        Command.Parameters.AddWithValue("@filter", filter);
                        Command.Parameters.AddWithValue("@orderBy", orderBy);
                        Command.Parameters.Add("@result", SqlDbType.Int);
                        Command.Parameters["@result"].Direction = ParameterDirection.Output;
                        Command.Parameters.Add("@errorMSG", SqlDbType.VarChar, 255);
                        Command.Parameters["@errorMSG"].Direction = ParameterDirection.Output;

                        //Perform the procedure and get the result
                        Connection.Open();
                        Reader = Command.ExecuteReader();

                        //read the player list, if an error occurred in the stored procedure there will be no results to read an this will be skipped
                        ModelFactory factory = new ModelFactory(Reader);
                        while (Reader.Read())
                        {
                            //Call the ModelFactory to build the model from the data
                            Player player = factory.PlayerFactory(false);

                            //If an error occurred while trying to build the player list
                            if (player == null)
                                return new Response<GamePlayerListResponse>(null, "ERROR", "An error occurred while trying to build the player list.", ErrorCodes.EC_BUILDMODELERROR);

                            players.Add(player);
                        }
                        Reader.Close();

                        //Get the output results from the stored procedure, Can only get the output results after the DataReader has been close
                        //The data reader will be closed after the last row of the results have been read.
                        Result = Convert.ToInt32(Command.Parameters["@result"].Value);
                        ErrorMSG = Convert.ToString(Command.Parameters["@errorMSG"].Value);

                        //If the response was not successful return the empty response
                        if(Result != 1)
                            return new Response<GamePlayerListResponse>(null, Result, ErrorMSG, Result);
                    }
                }

                //If the players list is empty, return an error
                if(players.Count == 0)
                    return new Response<GamePlayerListResponse>(null, "ERROR", "The players list is empty.", ErrorCodes.EC_PLAYERLIST_EMPTYLIST);

                //Get the Game object
                Response<Game> gameResponse = GetGameByID(players[0].GameID);

                //If the Game response was not successful, return an error
                if(!gameResponse.IsSuccessful())
                    return new Response<GamePlayerListResponse>(null, gameResponse.Type, gameResponse.ErrorMessage, gameResponse.ErrorCode);

                //Otherwise, create the successful response with the game and player list
                return new Response<GamePlayerListResponse>(new GamePlayerListResponse(gameResponse.Data, players), Result, ErrorMSG, Result);
            }

            //A database exception was thrown, return an error response
            catch
            {
                return new Response<GamePlayerListResponse>(null, "ERROR", DatabaseErrorMSG, ErrorCodes.EC_DATABASECONNECTERROR);
            }
        }







        /// <summary>
        /// Sets the Game State to starting in the database, updates the game start time to 10mins
        /// in the future and sets the Game end time. After this method is run code will be scheduled
        /// in the business logic to update the game to playing, then also update the game to be completed
        /// </summary>
        /// <param name="playerID">The ID of the host player of the game. Only the host player can begin the game.</param>
        /// <returns>The updated game object, NULL if an error occurred</returns>
        public Response<Game> BeginGame(int playerID)
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
                        Command.CommandType = CommandType.StoredProcedure;
                        Command.Parameters.AddWithValue("@playerID", playerID);
                        Command.Parameters.Add("@result", SqlDbType.Int);
                        Command.Parameters["@result"].Direction = ParameterDirection.Output;
                        Command.Parameters.Add("@errorMSG", SqlDbType.VarChar, 255);
                        Command.Parameters["@errorMSG"].Direction = ParameterDirection.Output;

                        //Perform the procedure and get the result
                        Connection.Open();
                        Reader = Command.ExecuteReader();
                        while (Reader.Read())
                        {
                            game = new ModelFactory(Reader).GameFactory();
                            if(game == null)
                                return new Response<Game>(null, "ERROR", "An error occurred while trying to build the Game model.", ErrorCodes.EC_BUILDMODELERROR);
                        }
                        Reader.Close();

                        //Format the results into a response object
                        Result = Convert.ToInt32(Command.Parameters["@result"].Value);
                        ErrorMSG = Convert.ToString(Command.Parameters["@errorMSG"].Value);
                        return new Response<Game>(game, Result, ErrorMSG, Result);
                    }
                }
            }
            //A database exception was thrown, return an error response
            catch
            {
                return new Response<Game>(null, "ERROR", DatabaseErrorMSG, ErrorCodes.EC_DATABASECONNECTERROR);
            }
        }




        /// <summary>
        /// Sets the GameState to the state passed in.
        /// </summary>
        /// <param name="gameID">The ID of the game to update</param>
        /// <param name="gameState">The new state to set.</param>
        /// <returns></returns>
        public Response<object> SetGameState(int gameID, string gameState)
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
                        Command.CommandType = CommandType.StoredProcedure;
                        Command.Parameters.AddWithValue("@gameID", gameID);
                        Command.Parameters.AddWithValue("@gameState", gameState);
                        Command.Parameters.Add("@result", SqlDbType.Int);
                        Command.Parameters["@result"].Direction = ParameterDirection.Output;
                        Command.Parameters.Add("@errorMSG", SqlDbType.VarChar, 255);
                        Command.Parameters["@errorMSG"].Direction = ParameterDirection.Output;

                        //Perform the procedure and get the result
                        Connection.Open();
                        Command.ExecuteNonQuery();

                        //Format the results into a response object
                        Result = Convert.ToInt32(Command.Parameters["@result"].Value);
                        ErrorMSG = Convert.ToString(Command.Parameters["@errorMSG"].Value);
                        return new Response<object>(null, Result, ErrorMSG, Result);
                    }
                }
            }
            //A database exception was thrown, return an error response
            catch
            {
                return new Response<object>(null, "ERROR", DatabaseErrorMSG, ErrorCodes.EC_DATABASECONNECTERROR);
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
        public Response<GameStatusResponse> GetGameStatus(int playerID)
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
                        Command.CommandType = CommandType.StoredProcedure;
                        Command.Parameters.AddWithValue("@playerID", playerID);
                        Command.Parameters.Add("@gameState", SqlDbType.VarChar, 255);
                        Command.Parameters["@gameState"].Direction = ParameterDirection.Output;
                        Command.Parameters.Add("@hasVotesToComplete", SqlDbType.Bit);
                        Command.Parameters["@hasVotesToComplete"].Direction = ParameterDirection.Output;
                        Command.Parameters.Add("@hasNotifications", SqlDbType.Bit);
                        Command.Parameters["@hasNotifications"].Direction = ParameterDirection.Output;
                        Command.Parameters.Add("@result", SqlDbType.Int);
                        Command.Parameters["@result"].Direction = ParameterDirection.Output;
                        Command.Parameters.Add("@errorMSG", SqlDbType.VarChar, 255);
                        Command.Parameters["@errorMSG"].Direction = ParameterDirection.Output;

                        //Perform the procedure and get the result
                        Connection.Open();
                        Reader = Command.ExecuteReader();

                        Player player = null;
                        while(Reader.Read())
                        {
                            player = new ModelFactory(Reader).PlayerFactory(true);
                            if(player == null)
                                return new Response<GameStatusResponse>(null, "ERROR", "An error occurred while trying to build the Player model.", ErrorCodes.EC_BUILDMODELERROR);
                        }
                        Reader.Close();

                        //Format the results into a response object
                        Result = Convert.ToInt32(Command.Parameters["@result"].Value);
                        ErrorMSG = Convert.ToString(Command.Parameters["@errorMSG"].Value);
                        response = new Response<GameStatusResponse>(null, Result, ErrorMSG, Result);

                        //If the result is not an error build the GameStatus object and assign it to the response
                        if(!IsError)
                        {
                            string gameState = Convert.ToString(Command.Parameters["@gameState"].Value);
                            bool hasVotesToComplete = Convert.ToBoolean(Command.Parameters["@hasVotesToComplete"].Value);
                            bool hasNotifications = Convert.ToBoolean(Command.Parameters["@hasNotifications"].Value);
                            GameStatusResponse gsr = new GameStatusResponse(gameState, hasVotesToComplete, hasNotifications, player);
                            response.Data = gsr;
                        }

                        return response;
                    }
                }
            }
            //A database exception was thrown, return an error response
            catch
            {
                return new Response<GameStatusResponse>(null, "ERROR", DatabaseErrorMSG, ErrorCodes.EC_DATABASECONNECTERROR);
            }
        }
    }
}
