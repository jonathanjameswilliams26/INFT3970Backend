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
        public Response<Game> BR_CreateGame(BRGame game)
        {
            StoredProcedure = "usp_CreateBRGame";
            try
            {
                //Create the connection and command for the stored procedure
                using (Connection = new SqlConnection(ConnectionString))
                {
                    using (Command = new SqlCommand(StoredProcedure, Connection))
                    {
                        //Add the procedure input and output params
                        AddParam("gameCode", game.GameCode);
                        AddParam("timeLimit", game.TimeLimit);
                        AddParam("ammoLimit", game.AmmoLimit);
                        AddParam("startDelay", game.StartDelay);
                        AddParam("replenishAmmoDelay", game.ReplenishAmmoDelay);
                        AddParam("gameMode", game.GameMode);
                        AddParam("isJoinableAtAnytime", game.IsJoinableAtAnytime);
                        AddParam("latitude", game.Latitude);
                        AddParam("longitude", game.Longitude);
                        AddParam("radius", game.Radius);
                        AddDefaultParams();

                        //Perform the procedure and get the result
                        RunReader();
                        while (Reader.Read())
                        {
                            game = (BRGame) new ModelFactory(Reader).GameFactory();
                            if (game == null)
                                return new Response<Game>("An error occurred while trying to build the Game model.", ErrorCodes.BUILD_MODEL_ERROR);
                        }
                        Reader.Close();

                        //Format the results into a response object
                        ReadDefaultParams();
                        return new Response<Game>(game, ErrorMSG, Result);
                    }
                }
            }
            //A database exception was thrown, return an error response
            catch
            {
                return Response<Game>.DatabaseErrorResponse();
            }
        }

        public Response<BRPlayer> BR_GetPlayerByID(int playerID)
        {
            //TODO: Implement Method
            throw new NotImplementedException();
        }

        public Response<BRPlayer> BR_UseAmmo(BRPlayer player)
        {
            //TODO: Implement method
            throw new NotImplementedException();
        }

        public Response<BRPlayer> BR_EliminatePlayer(BRPlayer player)
        {
            //TODO: Implement method
            throw new NotImplementedException();
        }
    }
}
