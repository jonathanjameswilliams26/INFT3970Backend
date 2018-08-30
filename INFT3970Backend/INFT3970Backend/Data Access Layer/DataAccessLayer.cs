using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace INFT3970Backend.Data_Access_Layer
{


    public class DataAccessLayer
    {
        //The connection string to the database
        protected const string ConnectionString = "Server=localhost;Database=udb_CamTag;Trusted_Connection=True;";

        //The error message returned when an exception is thrown when tryig to connect to the DB and run a stored procedure
        protected const string DatabaseErrorMSG = "An error occurred while trying to connect to the database.";


        protected SqlConnection Connection { get; set; }    //The connection to the database using the connection string
        protected SqlCommand Command { get; set; }          //The Command being run EG the Stored Procedure + Connection
        protected SqlDataReader Reader { get; set; }        //The reader object produced by the Command when reading data from SELECT statements
        protected string StoredProcedure{ get; set; }       //The StoredProcedure name being called
    }
}
