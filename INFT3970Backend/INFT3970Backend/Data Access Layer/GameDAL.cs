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
        /// Creates a new game using the game code passed in. Will return the gamecode upon successful execution.
        /// </summary>
        /// <param name="gameCode">The gamecode of the new game</param>
        /// <returns>
        /// A Response which contains the gameCode of the new game.
        /// Will return gameCode if gameCode does not exist and is added to the database.
        /// Will return a game code of NULL is an error occurs
        /// </returns>
        public Response<string> CreateGame(string gameCode)
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
                        Command.CommandType = CommandType.StoredProcedure;
                        Command.Parameters.AddWithValue("@gameCode", gameCode);
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
                        Response<string> response = new Response<string>(null, Result, ErrorMSG, Result);

                        //If the response is successful, set the data to be the correct game code
                        if (response.Type == ResponseType.SUCCESS)
                            response.Data = gameCode;

                        return response;
                    }
                }
            }

            //A database exception was thrown, return an error response
            catch
            {
                return new Response<string>(null, ResponseType.ERROR, DatabaseErrorMSG, ErrorCodes.EC_DATABASECONNECTERROR);
            }
        }







        /// <summary>
        /// Sets the game to inactive after a game was created by the host player and failed to join the game due to an unexpected
        /// error such as invalid contact details or the contact details are already taken by another player in an active playing game.
        /// </summary>
        /// <param name="gameCode">The game code being deactivated</param>
        public void DeactivateGameAfterHostJoinError(string gameCode)
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
                        Command.Parameters.AddWithValue("@gameCode", gameCode);

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
    }
}
