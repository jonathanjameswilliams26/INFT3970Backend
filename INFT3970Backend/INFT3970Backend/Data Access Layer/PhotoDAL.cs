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
                            Photo photo = factory.PhotoFactory();
                            if (photo == null)
                                return new Response<List<Photo>>(null, ResponseType.ERROR, "An error occurred while trying to build the photo list.", ErrorCodes.EC_BUILDMODELERROR);

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
                return new Response<List<Photo>>(null, ResponseType.ERROR, DatabaseErrorMSG, ErrorCodes.EC_DATABASECONNECTERROR);
            }
            
         
        }
    
    }
}
