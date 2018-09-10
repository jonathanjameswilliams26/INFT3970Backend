using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using INFT3970Backend.Models;

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
        public Game GetGameByID(int gameID)
        {
            StoredProcedure = "usp_GetGameByID";
            Game game = new Game();
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

                        //Perform the procedure and get the result
                        Connection.Open();
                        Reader = Command.ExecuteReader();
                        while(Reader.Read())
                        {
                            game = new ModelFactory(Reader).GameFactory();
                        }
                        Reader.Close();

                        return game;
                    }
                }
            }

            //A database exception was thrown, return an error response
            catch
            {
                return null;
            }
        }



        /// <summary>
        /// Creates a notification of type JOIN, sent to all players notifying.
        /// </summary>
        /// <param name="gameID">The gameID of the game</param>
        /// <param name="playerID">The playerID of who joined the game.</param>
        /// <returns>void</returns>
        public void CreateJoinNotification(int gameID, int playerID)
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
                        Command.Parameters.AddWithValue("@gameID", gameID);
                        Command.Parameters.AddWithValue("@playerID", playerID);
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
                
            }
        }




        /// <summary>
        /// Creates a notification of type LEAVE, sent to all players notifying.
        /// </summary>
        /// <param name="gameID">The gameID of the game</param>
        /// <param name="playerID">The playerID of who left the game.</param>
        /// <returns>void</returns>
        public void CreateLeaveNotification(int gameID, int playerID)
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
                        Command.Parameters.AddWithValue("@gameID", gameID);
                        Command.Parameters.AddWithValue("@playerID", playerID);
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

            }
        }



        /// <summary>
        /// Creates a notification of type LEAVE, sent to all players notifying.
        /// </summary>
        /// <param name="gameID">The gameID of the game</param>
        /// <param name="playerID">The playerID of who left the game.</param>
        /// <returns>void</returns>
        public void CreateTaggedNotification(int takenByID, int photoOfID)
        {
            StoredProcedure = "usp_CreateTaggedNotification";
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

            }
        }
    }
}
