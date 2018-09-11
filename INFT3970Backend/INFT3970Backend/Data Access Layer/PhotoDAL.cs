using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using INFT3970Backend.Models;
using System.Data;
using System.Data.SqlClient;

namespace INFT3970Backend.Data_Access_Layer
{
    public class PhotoDAL : DataAccessLayer
    {
        public PhotoDAL() { }

        public Response<List<Photo>> GetPhotoLocation(int photoID)
        {
            StoredProcedure = "usp_GetPlayerPhotoLocation";
            List<Photo> photos = new List<Photo>();
            try
            {
                //Create the connection and command for the stored procedure
                using (Connection = new SqlConnection(ConnectionString))
                {
                    using (Command = new SqlCommand(StoredProcedure, Connection))
                    {
                        //Add the procedure input and output params
                        Command.CommandType = CommandType.StoredProcedure;
                        Command.Parameters.AddWithValue("@photoID", photoID);
                        Command.Parameters.Add("@result", SqlDbType.Int);
                        Command.Parameters["@result"].Direction = ParameterDirection.Output;
                        Command.Parameters.Add("@errorMSG", SqlDbType.VarChar, 255);
                        Command.Parameters["@errorMSG"].Direction = ParameterDirection.Output;

                        //Perform the procedure and get the result
                        Connection.Open();
                        Reader = Command.ExecuteReader();
                     
                        while (Reader.Read())
                        {
                            ModelFactory factory = new ModelFactory(Reader);
                            Photo photo = factory.PhotoFactory(false, false, false);
                            if (photo == null)
                                return new Response<List<Photo>>(null, "ERROR", "An error occurred while trying to build the photo list.", ErrorCodes.EC_BUILDMODELERROR);

                            photos.Add(photo);
                        }
                        Reader.Close(); //Get the output results from the stored procedure, Can only get the output results after the DataReader has been close
                        //The data reader will be closed after the last row of the results have been read.
                        Result = Convert.ToInt32(Command.Parameters["@result"].Value);
                        ErrorMSG = Convert.ToString(Command.Parameters["@errorMSG"].Value);

                        //Format the results into a response object
                        return new Response<List<Photo>>(photos, Result, ErrorMSG, Result);


                    } 
                }
            }
            catch
            {
                return new Response<List<Photo>>(null, "ERROR", DatabaseErrorMSG, ErrorCodes.EC_DATABASECONNECTERROR);
            }
        }




        /// <summary>
        /// Saves a photo to the database, creates all PlayerVotePhoto records and returns the created Photo record in the DB.
        /// </summary>
        /// <param name="photo">The photo object to save to the database.</param>
        /// <returns></returns>
        public Response<Photo> SavePhoto(Photo photoToSave)
        {
            StoredProcedure = "usp_SavePhoto";
            Photo photo = null;
            try
            {
                //Create the connection and command for the stored procedure
                using (Connection = new SqlConnection(ConnectionString))
                {
                    using (Command = new SqlCommand(StoredProcedure, Connection))
                    {
                        //Add the procedure input and output params
                        Command.CommandType = CommandType.StoredProcedure;
                        Command.Parameters.AddWithValue("@dataURL", photoToSave.PhotoDataURL);
                        Command.Parameters.AddWithValue("@takenByID", photoToSave.TakenByPlayerID);
                        Command.Parameters.AddWithValue("@photoOfID", photoToSave.PhotoOfPlayerID);
                        Command.Parameters.AddWithValue("@lat", photoToSave.Lat);
                        Command.Parameters.AddWithValue("@long", photoToSave.Long);
                        Command.Parameters.Add("@result", SqlDbType.Int);
                        Command.Parameters["@result"].Direction = ParameterDirection.Output;
                        Command.Parameters.Add("@errorMSG", SqlDbType.VarChar, 255);
                        Command.Parameters["@errorMSG"].Direction = ParameterDirection.Output;

                        //Perform the procedure and get the result
                        Connection.Open();
                        Reader = Command.ExecuteReader();

                        while (Reader.Read())
                        {
                            ModelFactory factory = new ModelFactory(Reader);
                            photo = factory.PhotoFactory(false, false, false);
                            if (photo == null)
                                return new Response<Photo>(null, "ERROR", "An error occurred while trying to build the photo model.", ErrorCodes.EC_BUILDMODELERROR);
                        }
                        Reader.Close(); 
                        
                        //Get the output results from the stored procedure, Can only get the output results after the DataReader has been close
                        //The data reader will be closed after the last row of the results have been read.
                        Result = Convert.ToInt32(Command.Parameters["@result"].Value);
                        ErrorMSG = Convert.ToString(Command.Parameters["@errorMSG"].Value);

                        //Format the results into a response object
                        return new Response<Photo>(photo, Result, ErrorMSG, Result);
                    }
                }
            }
            catch
            {
                return new Response<Photo>(null, "ERROR", DatabaseErrorMSG, ErrorCodes.EC_DATABASECONNECTERROR);
            }
        }






        /// <summary>
        /// Gets a photo from the database matching the specified ID.
        /// Returns the Photo matching the ID. NULL if the photo does not exist.
        /// </summary>
        /// <param name="id">The ID of the photo to get.</param>
        /// <returns></returns>
        public Photo GetPhotoByID(int id)
        {
            StoredProcedure = "usp_GetPhotoByID";
            Photo photo = null;
            try
            {
                //Create the connection and command for the stored procedure
                using (Connection = new SqlConnection(ConnectionString))
                {
                    using (Command = new SqlCommand(StoredProcedure, Connection))
                    {
                        //Add the procedure input and output params
                        Command.CommandType = CommandType.StoredProcedure;
                        Command.Parameters.AddWithValue("@photoID", id);

                        //Perform the procedure and get the result
                        Connection.Open();
                        Reader = Command.ExecuteReader();

                        while (Reader.Read())
                        {
                            ModelFactory factory = new ModelFactory(Reader);
                            photo = factory.PhotoFactory(true, true, true);
                        }
                        Reader.Close();

                        //Format the results into a response object
                        return photo;
                    }
                }
            }
            catch
            {
                return null;
            }
        }





        /// <summary>
        /// Updates the photo record after the voting time has expried. If all players
        /// have not voted on the photo it is an automatic successful photo.
        /// Returns the updated photo record. NULL if error or ID does not exist.
        /// </summary>
        /// <param name="photoID">The ID of the photo to update.</param>
        /// <returns></returns>
        public Response<Photo> VotingTimeExpired(int photoID)
        {
            StoredProcedure = "usp_VotingTimeExpired";
            Photo photo = null;
            try
            {
                //Create the connection and command for the stored procedure
                using (Connection = new SqlConnection(ConnectionString))
                {
                    using (Command = new SqlCommand(StoredProcedure, Connection))
                    {
                        //Add the procedure input and output params
                        Command.CommandType = CommandType.StoredProcedure;
                        Command.Parameters.AddWithValue("@photoID", photoID);
                        Command.Parameters.Add("@result", SqlDbType.Int);
                        Command.Parameters["@result"].Direction = ParameterDirection.Output;
                        Command.Parameters.Add("@errorMSG", SqlDbType.VarChar, 255);
                        Command.Parameters["@errorMSG"].Direction = ParameterDirection.Output;

                        //Perform the procedure and get the result
                        Connection.Open();
                        Reader = Command.ExecuteReader();

                        while (Reader.Read())
                        {
                            ModelFactory factory = new ModelFactory(Reader);
                            photo = factory.PhotoFactory(true, true, true);
                        }
                        Reader.Close();

                        //Get the output results from the stored procedure, Can only get the output results after the DataReader has been close
                        //The data reader will be closed after the last row of the results have been read.
                        Result = Convert.ToInt32(Command.Parameters["@result"].Value);
                        ErrorMSG = Convert.ToString(Command.Parameters["@errorMSG"].Value);

                        //Format the results into a response object
                        return new Response<Photo>(photo, Result, ErrorMSG, Result);
                    }
                }
            }
            catch
            {
                return new Response<Photo>(null, "ERROR", DatabaseErrorMSG, ErrorCodes.EC_DATABASECONNECTERROR);
            }
        }





        /// <summary>
        /// Gets the list of votes the player must complete / the PlayerVotePhoto records which have not been completed by the player.
        /// </summary>
        /// <param name="playerID">The playerID which PlayerVotePhotoRecords to get</param>
        /// <returns></returns>
        public Response<List<PlayerVotePhoto>> GetVotesToComplete(int playerID)
        {
            StoredProcedure = "usp_GetVotesToComplete";
            List<PlayerVotePhoto> list = new List<PlayerVotePhoto>();
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
                            PlayerVotePhoto playerVotePhoto = new ModelFactory(Reader).PlayerVotePhotoFactory(true, true);
                            if (playerVotePhoto == null)
                                return new Response<List<PlayerVotePhoto>>(null, "ERROR", "An error occurred while trying to build the model.", ErrorCodes.EC_BUILDMODELERROR);
                            else
                                list.Add(playerVotePhoto);
                        }
                        Reader.Close();

                        //Get the output results from the stored procedure, Can only get the output results after the DataReader has been close
                        //The data reader will be closed after the last row of the results have been read.
                        Result = Convert.ToInt32(Command.Parameters["@result"].Value);
                        ErrorMSG = Convert.ToString(Command.Parameters["@errorMSG"].Value);

                        //Format the results into a response object
                        return new Response<List<PlayerVotePhoto>>(list, Result, ErrorMSG, Result);
                    }
                }
            }
            catch
            {
                return new Response<List<PlayerVotePhoto>>(null, "ERROR", DatabaseErrorMSG, ErrorCodes.EC_DATABASECONNECTERROR);
            }
        }
    }
}
