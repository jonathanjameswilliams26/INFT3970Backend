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
        /// <returns>A list of Player objects inside the game which the passed in playerID is in.</returns>
        public Response<List<Player>> GetGamePlayerList(int playerID, bool doGetGameDetails)
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
                            Player player = factory.PlayerFactory(doGetGameDetails);

                            //If an error occurred while trying to build the player list
                            if(player == null)
                                return new Response<List<Player>>(null, "ERROR", "An error occurred while trying to build the player list.", ErrorCodes.EC_BUILDMODELERROR);

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
                return new Response<List<Player>>(null, "ERROR", DatabaseErrorMSG, ErrorCodes.EC_DATABASECONNECTERROR);
            }
        }





        /// <summary>
        /// Updates the specified playerID's connectionID inside the database.
        /// Sets the ConnectionID to the ID passed in and sets IsConnected = TRUE
        /// </summary>
        /// <param name="playerID">The player being updated</param>
        /// <param name="connectionID">The new connectionID</param>
        public void UpdateConnectionID(int playerID, string connectionID)
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




        /// <summary>
        /// Adds the player to the game matching the gamecode passed in.
        /// Returns the created Player object. NULL data if error occurred.
        /// </summary>
        /// <param name="gameCode">The gamecode the player is attempting to join</param>
        /// <param name="nickname">The nickname chosen by the player</param>
        /// <param name="contact">The email or phone number entered by the player to receive notifications</param>
        /// <param name="isPhone">Flag value which outlines if the contact passed in is a phone number or email. TRUE = phone number, FALSE = email</param>
        /// <returns>The created Player object or NULL data if error occurred.</returns>
        public Response<Player> JoinGame(string gameCode, string nickname, string contact, bool isPhone, int verificationCode, bool isHost)
        {
            StoredProcedure = "usp_JoinGame";
            Player player = null;
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

                        //Perform the procedure and get the result
                        Connection.Open();
                        Reader = Command.ExecuteReader();
                        while(Reader.Read())
                        {
                            player = new ModelFactory(Reader).PlayerFactory(true);
                            if (player == null)
                                return new Response<Player>(null, "ERROR", "An error occurred while trying to build the player model.", ErrorCodes.EC_BUILDMODELERROR);
                        }
                        Reader.Close();

                        //Format the results into a response object
                        Result = Convert.ToInt32(Command.Parameters["@result"].Value);
                        ErrorMSG = Convert.ToString(Command.Parameters["@errorMSG"].Value);
                        return new Response<Player>(player, Result, ErrorMSG, Result);
                    }
                }
            }

            //A database exception was thrown, return an error response
            catch
            {
                return new Response<Player>(null, "ERROR", DatabaseErrorMSG, ErrorCodes.EC_DATABASECONNECTERROR);
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
                return new Response<object>(null, "ERROR", DatabaseErrorMSG, ErrorCodes.EC_DATABASECONNECTERROR);
            }
        }







        /// <summary>
        /// Updates the Player's verification code.
        /// Returns a Response with string data, where the data is the contact (phone or email) of the playerID in
        /// order to resend the verification code.
        /// </summary>
        /// <param name="playerID">The playerID who's verification code is being updated</param>
        /// <param name="verificationCode">The new verification code</param>
        /// <returns>The email or phone number to send the new verification code to. NULL data if error.</returns>
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
                return new Response<string>(null, "ERROR", DatabaseErrorMSG, ErrorCodes.EC_DATABASECONNECTERROR);
            }
        }





        /// <summary>
        /// Removes the ConnectionID from the Player record when the player disconnects from the application hub.
        /// Sets the player's ConnectionID to NULL and IsConnected to FALSE.
        /// </summary>
        /// <param name="connectionID">The connectionID to remove.</param>
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





        /// <summary>
        /// Gets a list of all notifications for the passed player
        /// </summary>
        /// <param name="playerID"></param>
        /// <returns>A list of Notification objects for the respective playerID.</returns>
        public Response<List<Notification>> GetNotificationList(int playerID, bool all)
        {
            StoredProcedure = "usp_GetNotifications";
            List<Notification> notifs = new List<Notification>();
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
                        Command.Parameters.AddWithValue("@all", all);
                        Command.Parameters.Add("@result", SqlDbType.Int);
                        Command.Parameters["@result"].Direction = ParameterDirection.Output;
                        Command.Parameters.Add("@errorMSG", SqlDbType.VarChar, 255);
                        Command.Parameters["@errorMSG"].Direction = ParameterDirection.Output;

                        //Perform the procedure and get the result
                        Connection.Open();
                        Reader = Command.ExecuteReader();


                        //read the notif list, if an error occurred in the stored procedure there will be no results to read an this will be skipped
                        while (Reader.Read())
                        {
                            //Call the ModelFactory to build the model from the data
                            ModelFactory factory = new ModelFactory(Reader);

                            Notification notification = factory.NotificationFactory();

                            //If an error occurred while trying to build the notification list
                            if (notification == null)
                                return new Response<List<Notification>>(null, "ERROR", "An error occurred while trying to build the notification list.", ErrorCodes.EC_BUILDMODELERROR);

                            notifs.Add(notification);
                        }
                        Reader.Close();

                        //Get the output results from the stored procedure, Can only get the output results after the DataReader has been close
                        //The data reader will be closed after the last row of the results have been read.
                        Result = Convert.ToInt32(Command.Parameters["@result"].Value);
                        ErrorMSG = Convert.ToString(Command.Parameters["@errorMSG"].Value);

                        //Format the results into a response object
                        return new Response<List<Notification>>(notifs, Result, ErrorMSG, Result);
                    }
                }
            }

            //A database exception was thrown, return an error response
            catch
            {
                return new Response<List<Notification>>(null, "ERROR", DatabaseErrorMSG, ErrorCodes.EC_DATABASECONNECTERROR);
            }
        }




        /// <summary>
        /// Leaves a player from their active game
        /// </summary>
        /// <param name="playerID">The playerID used to determine which player is leaving the game.</param>
        /// <returns>A response status.</returns>
        public Response<int> LeaveGame(int playerID)
        {
            StoredProcedure = "usp_LeaveGame";
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

                        //Get the output results from the stored procedure, Can only get the output results after the DataReader has been close
                        //The data reader will be closed after the last row of the results have been read.
                        Result = Convert.ToInt32(Command.Parameters["@result"].Value);
                        ErrorMSG = Convert.ToString(Command.Parameters["@errorMSG"].Value);

                        //Format the results into a response object
                        return new Response<int>(1, Result, ErrorMSG, Result);
                    }
                }
            }

            //A database exception was thrown, return an error response
            catch
            {
                return new Response<int>(-1, "ERROR", DatabaseErrorMSG, ErrorCodes.EC_DATABASECONNECTERROR);
            }
        }
    }
}
