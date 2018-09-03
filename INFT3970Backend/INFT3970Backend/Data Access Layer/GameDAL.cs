using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using INFT3970Backend.Models;

namespace INFT3970Backend.Data_Access_Layer
{
    public class GameDAL : DataAccessLayer
    {
        /// <summary>
        /// Creates a new game and checks to see if the gameCode exists already. If so, return existing error.
        /// </summary>
        /// <param name="gameCode">The gamecode of the new game</param>
        /// <returns>
        /// A Response which contains the gameCode of the new game.
        /// Will return gameCode if gameCode does not exist and is added.
        /// Will return a gameCode of -1 if an error occurred.
        /// </returns>
        public Response<string> CreateGame(string gameCode)
        {
            StoredProcedure = "usp_CreateGame";
            try
            {
                //
                //
                //
                gameCode = "abc123";

                return new Response<string>(gameCode, ResponseType.SUCCESS, ErrorMSG, Result);
            }

            //A database exception was thrown, return an error response
            catch
            {
                return new Response<string>("-1", ResponseType.ERROR, DatabaseErrorMSG, ErrorCodes.EC_DATABASECONNECTERROR);
            }
        }






        /// <summary>
        /// Checks to see if game exists with new gameCode.
        /// </summary>
        /// <param name="gameCode">The gamecode of the new game</param>
        /// <returns>
        /// A Response which contains the gameCode of the new game.
        /// Will return a ITEMALREADYEXISTS error if gameCode exists.
        /// Will return a DATABASECONNECTERROR error if an error occurred.
        /// Will return a 1 if no gameCode exists.
        /// </returns>
        public Response<string> CheckGameCode(string gameCode)
        {
            StoredProcedure = "usp_CheckGameCode";
            try
            {
                //
                //
                //
                int Result = 1;
                string Result2 = "1";

                return new Response<string>(Result2, ResponseType.SUCCESS, ErrorMSG, Result);
            }

            //A database exception was thrown, return an error response
            catch
            {
                return new Response<string>("-1", ResponseType.ERROR, DatabaseErrorMSG, ErrorCodes.EC_DATABASECONNECTERROR);
            }
        }
    }
}
