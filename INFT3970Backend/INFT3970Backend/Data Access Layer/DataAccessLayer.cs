using System;
using System.Data;
using System.Data.SqlClient;

namespace INFT3970Backend.Data_Access_Layer
{
    public class DataAccessLayer
    {
        //The connection string to the database
        protected const string ConnectionString = @"Server=.\SQLEXPRESS;Database=udb_CamTag;Trusted_Connection=True;";
        //protected const string ConnectionString = @"Server=.\SQLEXPRESS;Database=udb_CamTag;User Id=DataAccessLayerLogin;Password=test";

        //The error message returned when an exception is thrown when tryig to connect to the DB and run a stored procedure
        protected const string DatabaseErrorMSG = "An error occurred while trying to connect to the database.";

        protected SqlConnection Connection { get; set; }    //The connection to the database using the connection string
        protected SqlCommand Command { get; set; }          //The Command being run EG the Stored Procedure + Connection
        protected SqlDataReader Reader { get; set; }        //The reader object produced by the Command when reading data from SELECT statements
        protected string StoredProcedure { get; set; }      //The StoredProcedure name being called
        protected int Result { get; set; }                  //The result of the stored procedure execution 1 = successful, anthing else = an error occurred
        protected string ErrorMSG { get; set; }             //The error message returned by the stored procedure
        protected int ErrorCode { get; set; }               //The error code return by the stored procedure
        protected bool IsError                              //The flag which outlines if an error occurred while running the stored procedure
        {
            get
            {
                if (Result != 1)
                    return true;
                else
                    return false;
            }
        }


        protected void Run()
        {
            Command.CommandType = CommandType.StoredProcedure;
            Connection.Open();
            Command.ExecuteNonQuery();
        }

        protected void RunReader()
        {
            Command.CommandType = CommandType.StoredProcedure;
            Connection.Open();
            Reader = Command.ExecuteReader();
        }


        protected void AddDefaultParams()
        {
            Command.Parameters.Add("@result", SqlDbType.Int);
            Command.Parameters["@result"].Direction = ParameterDirection.Output;
            Command.Parameters.Add("@errorMSG", SqlDbType.VarChar, 255);
            Command.Parameters["@errorMSG"].Direction = ParameterDirection.Output;
        }


        protected void ReadDefaultParams()
        {
            Result = Convert.ToInt32(Command.Parameters["@result"].Value);
            ErrorMSG = Convert.ToString(Command.Parameters["@errorMSG"].Value);
        }

        protected void AddParam(string name, object value)
        {
            Command.Parameters.AddWithValue("@" + name, value);
        }

        protected void AddOutput(string name, SqlDbType type)
        {
            if(type == SqlDbType.VarChar)
            {
                Command.Parameters.Add("@" + name, SqlDbType.VarChar, 255);
                Command.Parameters["@" + name].Direction = ParameterDirection.Output;
            }
            else
            {
                Command.Parameters.Add("@" + name, type);
                Command.Parameters["@" + name].Direction = ParameterDirection.Output;
            }
        }
    }
}
