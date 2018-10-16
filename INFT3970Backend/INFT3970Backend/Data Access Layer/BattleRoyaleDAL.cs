using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using INFT3970Backend.Models;
using INFT3970Backend.Models.Responses;

namespace INFT3970Backend.Data_Access_Layer
{
    public class BattleRoyaleDAL : DataAccessLayer
    {
        public Response<Player> BR_DisableOrRenablePlayer(Player player, int disabledForMinutes)
        {
            StoredProcedure = "usp_BR_DisablePlayer";
            try
            {
                //Create the connection and command for the stored procedure
                using (Connection = new SqlConnection(ConnectionString))
                {
                    using (Command = new SqlCommand(StoredProcedure, Connection))
                    {
                        //Add the procedure input and output params
                        AddParam("playerID", player.PlayerID);
                        AddParam("isAlreadyDisabled", player.IsDisabled);
                        AddParam("disabledForMinutes", disabledForMinutes);
                        AddDefaultParams();

                        //Perform the procedure and get the result
                        RunReader();
                        while (Reader.Read())
                        {
                            player = new ModelFactory(Reader).PlayerFactory(true);
                            if (player == null)
                                return new Response<Player>("An error occurred while trying to build the player model.", ErrorCodes.BUILD_MODEL_ERROR);
                        }
                        Reader.Close();

                        //Format the results into a response object
                        ReadDefaultParams();
                        return new Response<Player>(player, ErrorMSG, Result);
                    }
                }
            }

            //A database exception was thrown, return an error response
            catch
            {
                return Response<Player>.DatabaseErrorResponse();
            }
        }
    }
}
