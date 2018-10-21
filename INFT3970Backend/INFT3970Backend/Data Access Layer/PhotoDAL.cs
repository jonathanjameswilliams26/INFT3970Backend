using System.Collections.Generic;
using INFT3970Backend.Models;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using System;

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
                        AddParam("photoID", photoID);
                        AddDefaultParams();

                        //Perform the procedure and get the result
                        RunReader();
                        while (Reader.Read())
                        {
                            ModelFactory factory = new ModelFactory(Reader);
                            Photo photo = factory.PhotoFactory(false, false, false);
                            if (photo == null)
                                return new Response<List<Photo>>("An error occurred while trying to build the photo list.", ErrorCodes.BUILD_MODEL_ERROR);

                            photos.Add(photo);
                        }
                        Reader.Close();
                        
                        //Format the results into a response object
                        ReadDefaultParams();
                        return new Response<List<Photo>>(photos, ErrorMSG, Result);
                    } 
                }
            }
            catch
            {
                return Response<List<Photo>>.DatabaseErrorResponse();
            }
        }




        /// <summary>
        /// Saves a photo to the database, creates all PlayerVotePhoto records and returns the created Photo record in the DB.
        /// </summary>
        /// <param name="photo">The photo object to save to the database.</param>
        /// <returns></returns>
        public Response<Photo> SavePhoto(Photo photoToSave, bool isBR)
        {
            if(isBR)
                StoredProcedure = "usp_BR_SavePhoto";
            else
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
                        AddParam("dataURL", photoToSave.PhotoDataURL);
                        AddParam("takenByID", photoToSave.TakenByPlayerID);
                        AddParam("photoOfID", photoToSave.PhotoOfPlayerID);
                        AddParam("lat", photoToSave.Lat);
                        AddParam("long", photoToSave.Long);
                        AddDefaultParams();

                        //Perform the procedure and get the result
                        RunReader();
                        while (Reader.Read())
                        {
                            ModelFactory factory = new ModelFactory(Reader);
                            photo = factory.PhotoFactory(false, false, false);
                            if (photo == null)
                                return new Response<Photo>("An error occurred while trying to build the photo model.", ErrorCodes.BUILD_MODEL_ERROR);
                        }
                        Reader.Close();
                        
                        //Format the results into a response object
                        ReadDefaultParams();
                        return new Response<Photo>(photo, ErrorMSG, Result);
                    }
                }
            }
            catch
            {
                return Response<Photo>.DatabaseErrorResponse();
            }
        }





        public Response<List<Photo>> GetLastKnownLocations(Player player)
        {
            StoredProcedure = "usp_GetLastKnownLocations";
            List<Photo> photos = new List<Photo>();
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
                        ModelFactory factory = new ModelFactory(Reader);
                        while (Reader.Read())
                        {
                            var photo = factory.PhotoFactory(true, true, false);
                            if (photo == null)
                                return new Response<List<Photo>>("An error occurred while trying to build the list photo model.", ErrorCodes.BUILD_MODEL_ERROR);

                            photos.Add(photo);
                        }
                        Reader.Close();
                        
                        //Format the results into a response object
                        ReadDefaultParams();
                        return new Response<List<Photo>>(photos, ErrorMSG, Result);
                    }
                }
            }
            catch
            {
                return Response<List<Photo>>.DatabaseErrorResponse();
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
                        
                        AddParam("photoID", id);

                        //Perform the procedure and get the result
                        RunReader();
                        

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
        public Response<Photo> VotingTimeExpired(int photoID, bool isBR)
        {
            if (isBR)
                StoredProcedure = "usp_BR_VotingTimeExpired";
            else
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
                        AddParam("photoID", photoID);
                        AddDefaultParams();

                        //Perform the procedure and get the result
                        RunReader();
                        while (Reader.Read())
                        {
                            ModelFactory factory = new ModelFactory(Reader);
                            photo = factory.PhotoFactory(true, true, true);
                        }
                        Reader.Close();
                        
                        //Format the results into a response object
                        ReadDefaultParams();
                        return new Response<Photo>(photo, ErrorMSG, Result);
                    }
                }
            }
            catch
            {
                return Response<Photo>.DatabaseErrorResponse();
            }
        }





        /// <summary>
        /// Gets the list of votes the player must complete / the PlayerVotePhoto records which have not been completed by the player.
        /// </summary>
        /// <param name="playerID">The playerID which PlayerVotePhotoRecords to get</param>
        /// <returns></returns>
        public Response<List<Vote>> GetVotesToComplete(Player player)
        {
            StoredProcedure = "usp_GetVotesToComplete";
            List<Vote> list = new List<Vote>();
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
                            Vote playerVotePhoto = new ModelFactory(Reader).PlayerVotePhotoFactory(true, true);
                            if (playerVotePhoto == null)
                                return new Response<List<Vote>>("An error occurred while trying to build the model.", ErrorCodes.BUILD_MODEL_ERROR);
                            else
                                list.Add(playerVotePhoto);
                        }
                        Reader.Close();
                        
                        //Format the results into a response object
                        ReadDefaultParams();
                        return new Response<List<Vote>>(list, ErrorMSG, Result);
                    }
                }
            }
            catch
            {
                return Response<List<Vote>>.DatabaseErrorResponse();
            }
        }




        public Response<Vote> VoteOnPhoto(Vote playerVote, bool isBR)
        {
            if (isBR)
                StoredProcedure = "usp_BR_VoteOnPhoto";
            else
                StoredProcedure = "usp_VoteOnPhoto";
            Vote playerVotePhoto = null;
            try
            {
                //Create the connection and command for the stored procedure
                using (Connection = new SqlConnection(ConnectionString))
                {
                    using (Command = new SqlCommand(StoredProcedure, Connection))
                    {
                        //Add the procedure input and output params
                        AddParam("voteID", playerVote.VoteID);
                        AddParam("playerID", playerVote.PlayerID);
                        AddParam("isPhotoSuccessful", playerVote.IsPhotoSuccessful);
                        AddDefaultParams();

                        //Perform the procedure and get the result
                        RunReader();
                        while (Reader.Read())
                        {
                            playerVotePhoto = new ModelFactory(Reader).PlayerVotePhotoFactory(true, true);
                            if (playerVotePhoto == null)
                                return new Response<Vote>("An error occurred while trying to build the model.", ErrorCodes.BUILD_MODEL_ERROR);
                        }
                        Reader.Close();
                        
                        //Format the results into a response object
                        ReadDefaultParams();
                        return new Response<Vote>(playerVotePhoto, ErrorMSG, Result);
                    }
                }
            }
            catch
            {
                return Response<Vote>.DatabaseErrorResponse();
            }
        }
    }
}
