using INFT3970Backend.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Backend.Data_Access_Layer
{
    public class PlayerDAL: DataAccessLayer
    {
        public PlayerDAL()
        {

        }



        /// <summary>
        /// Gets a list of all the players in the same game as the playerID passed in
        /// </summary>
        /// <param name="playerID"></param>
        /// <returns>A list of players inside the game which the passed in playerID is in.</returns>
        public Response<List<Player>> GetGamePlayerList(int playerID)
        {
            StoredProcedure = "usp_GetGamePlayerList";
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
                        Command.Parameters.AddWithValue("@playerID", playerID);
                        Command.Parameters.Add("@result", SqlDbType.Int);
                        Command.Parameters["@result"].Direction = ParameterDirection.Output;
                        Command.Parameters.Add("@errorMSG", SqlDbType.VarChar, 255);
                        Command.Parameters["@errorMSG"].Direction = ParameterDirection.Output;

                        //Perform the procedure and get the result
                        Connection.Open();
                        Reader = Command.ExecuteReader();


                        //read the player list, if an error occurred in the stored procedure there will be no results to read an this will be skipped
                        while (Reader.Read())
                        {
                            //Call the ModelFactory to build the model from the data
                            ModelFactory factory = new ModelFactory(Reader);
                            Player player = factory.PlayerFactory();

                            //If an error occurred while trying to build the player list
                            if(player == null)
                                return new Response<List<Player>>(null, ResponseType.ERROR, "An error occurred while trying to build the player list.", ErrorCodes.EC_BUILDMODELERROR);

                            players.Add(player);
                        }
                        Reader.Close();

                        //Get the output results from the stored procedure, Can only get the output results after the DataReader has been close
                        //The data reader will be closed after the last row of the results have been read.
                        Result = Convert.ToInt32(Command.Parameters["@result"].Value);
                        ErrorMSG = Convert.ToString(Command.Parameters["@errorMSG"].Value);

                        //Format the results into a response object
                        return new Response<List<Player>>(players, Result, ErrorMSG, Result);
                    }
                }
            }

            //A database exception was thrown, return an error response
            catch
            {
                return new Response<List<Player>>(null, ResponseType.ERROR, DatabaseErrorMSG, ErrorCodes.EC_DATABASECONNECTERROR);
            }
        }





        /// <summary>
        /// Updates the specified playerID's connectionID inside the database
        /// </summary>
        /// <param name="playerID">The player being updated</param>
        /// <param name="connectionID">The new connectionID</param>
        /// <returns></returns>
        public Response<object> UpdateConnectionID(int playerID, string connectionID)
        {
            StoredProcedure = "usp_UpdateConnectionID";
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
                        Command.Parameters.AddWithValue("@connectionID", connectionID);
                        Command.Parameters.Add("@result", SqlDbType.Int);
                        Command.Parameters["@result"].Direction = ParameterDirection.Output;
                        Command.Parameters.Add("@errorMSG", SqlDbType.VarChar,255);
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
                return new Response<object>(null, ResponseType.ERROR, DatabaseErrorMSG, ErrorCodes.EC_DATABASECONNECTERROR);
            }
        }




        /// <summary>
        /// Adds the player to the game.
        /// </summary>
        /// <param name="gameCode">The gamecode the player is attempting to join</param>
        /// <param name="nickname">The nickname chosen by the player</param>
        /// <param name="contact">The email or phone number entered by the player to receive notifications</param>
        /// <param name="isPhone">Flag value which outlines if the contact passed in is a phone number or email. TRUE = phone number, FALSE = email</param>
        /// <returns>
        /// A Response which contains the playerID generated in the database once the player has joined the game.
        /// Will return a playerID of -1 if an error occurred
        /// </returns>
        public Response<int> JoinGame(string gameCode, string nickname, string contact, bool isPhone, int verificationCode, bool isHost)
        {
            StoredProcedure = "usp_JoinGame";
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
                        Command.Parameters.AddWithValue("@nickname", nickname);
                        Command.Parameters.AddWithValue("@contact", contact);
                        Command.Parameters.AddWithValue("@isPhone", isPhone);
                        Command.Parameters.AddWithValue("@verificationCode", verificationCode);
                        Command.Parameters.AddWithValue("@isHost", isHost);
                        Command.Parameters.Add("@result", SqlDbType.Int);
                        Command.Parameters["@result"].Direction = ParameterDirection.Output;
                        Command.Parameters.Add("@errorMSG", SqlDbType.VarChar, 255);
                        Command.Parameters["@errorMSG"].Direction = ParameterDirection.Output;
                        Command.Parameters.Add("@createdPlayerID", SqlDbType.Int);
                        Command.Parameters["@createdPlayerID"].Direction = ParameterDirection.Output;

                        //Perform the procedure and get the result
                        Connection.Open();
                        Command.ExecuteNonQuery();

                        //Format the results into a response object
                        Result = Convert.ToInt32(Command.Parameters["@result"].Value);
                        ErrorMSG = Convert.ToString(Command.Parameters["@errorMSG"].Value);
                        int createdPlayerID = Convert.ToInt32(Command.Parameters["@createdPlayerID"].Value);
                        return new Response<int>(createdPlayerID, Result, ErrorMSG, Result);
                    }
                }
            }

            //A database exception was thrown, return an error response
            catch
            {
                return new Response<int>(-1, ResponseType.ERROR, DatabaseErrorMSG, ErrorCodes.EC_DATABASECONNECTERROR);
            }
        }






        /// <summary>
        /// Confirms the validation code entered by the player matches what is stored against their player record.
        /// If the player successfully enters the validation code their player record will be set to "Verified"
        /// </summary>
        /// <param name="verificationCode">The verification code to confirm is correct</param>
        /// <param name="playerID">The ID of the player to verify.</param>
        /// <returns></returns>
        public Response<object> ValidateVerificationCode(int verificationCode, int playerID)
        {
            StoredProcedure = "usp_ValidateVerificationCode";
            try
            {
                //Create the connection and command for the stored procedure
                using (Connection = new SqlConnection(ConnectionString))
                {
                    using (Command = new SqlCommand(StoredProcedure, Connection))
                    {
                        //Add the procedure input and output params
                        Command.CommandType = CommandType.StoredProcedure;
                        Command.Parameters.AddWithValue("@verificationCode", verificationCode);
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
                return new Response<object>(null, ResponseType.ERROR, DatabaseErrorMSG, ErrorCodes.EC_DATABASECONNECTERROR);
            }
        }







        /// <summary>
        /// Updates the Player's verification code.
        /// Returns a Response with string data, where the data is the contact (phone or email) of the playerID in
        /// order to resend the verification code.
        /// </summary>
        /// <param name="playerID">The playerID who's verification code is being updated</param>
        /// <param name="verificationCode">The new verification code</param>
        /// <returns></returns>
        public Response<string> UpdateVerificationCode(int playerID, int verificationCode)
        {
            StoredProcedure = "usp_UpdateVerificationCode";
            try
            {
                //Create the connection and command for the stored procedure
                using (Connection = new SqlConnection(ConnectionString))
                {
                    using (Command = new SqlCommand(StoredProcedure, Connection))
                    {
                        //Add the procedure input and output params
                        Command.CommandType = CommandType.StoredProcedure;
                        Command.Parameters.AddWithValue("@verificationCode", verificationCode);
                        Command.Parameters.AddWithValue("@playerID", playerID);
                        Command.Parameters.Add("@result", SqlDbType.Int);
                        Command.Parameters["@result"].Direction = ParameterDirection.Output;
                        Command.Parameters.Add("@errorMSG", SqlDbType.VarChar, 255);
                        Command.Parameters["@errorMSG"].Direction = ParameterDirection.Output;
                        Command.Parameters.Add("@phone", SqlDbType.VarChar, 255);
                        Command.Parameters["@phone"].Direction = ParameterDirection.Output;
                        Command.Parameters.Add("@email", SqlDbType.VarChar, 255);
                        Command.Parameters["@email"].Direction = ParameterDirection.Output;

                        //Perform the procedure and get the result
                        Connection.Open();
                        Command.ExecuteNonQuery();

                        //Format the results into a response object
                        Result = Convert.ToInt32(Command.Parameters["@result"].Value);
                        ErrorMSG = Convert.ToString(Command.Parameters["@errorMSG"].Value);
                        string phone = Convert.ToString(Command.Parameters["@phone"].Value);
                        string email = Convert.ToString(Command.Parameters["@email"].Value);

                        //Return the phone or email address to send the new verification to.
                        if (!String.IsNullOrWhiteSpace(phone))
                            return new Response<string>(phone, Result, ErrorMSG, Result);
                        else if (!String.IsNullOrWhiteSpace(email))
                            return new Response<string>(email, Result, ErrorMSG, Result);
                        else
                            return new Response<string>(null, Result, ErrorMSG, Result);
                    }
                }
            }

            //A database exception was thrown, return an error response
            catch
            {
                return new Response<string>(null, ResponseType.ERROR, DatabaseErrorMSG, ErrorCodes.EC_DATABASECONNECTERROR);
            }
        }






        public void RemoveConnectionID(string connectionID)
        {
            StoredProcedure = "usp_RemoveConnectionID";
            try
            {
                //Create the connection and command for the stored procedure
                using (Connection = new SqlConnection(ConnectionString))
                {
                    using (Command = new SqlCommand(StoredProcedure, Connection))
                    {
                        //Add the procedure input and output params
                        Command.CommandType = CommandType.StoredProcedure;
                        Command.Parameters.AddWithValue("@connectionID", connectionID);

                        //Perform the procedure and get the result
                        Connection.Open();
                        Command.ExecuteNonQuery();
                    }
                }
            }

            //A database exception was thrown
            catch
            {
                //Do nothing
            }
        }
    }
}
