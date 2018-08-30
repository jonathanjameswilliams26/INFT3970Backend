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
                        int result = Convert.ToInt32(Command.Parameters["@result"].Value);
                        string errorMSG = Convert.ToString(Command.Parameters["@errorMSG"].Value);
                        return new Response<object>(null, result, errorMSG);
                    }
                }
            }

            //A database exception was thrown, return an error response
            catch
            {
                return new Response<object>(null, ResponseType.ERROR, DatabaseErrorMSG);
            }
        }
    }
}
