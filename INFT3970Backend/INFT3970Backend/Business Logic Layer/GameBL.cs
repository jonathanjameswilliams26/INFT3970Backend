using INFT3970Backend.Data_Access_Layer;
using INFT3970Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace INFT3970Backend.Business_Logic_Layer
{
    public class GameBL
    {
        /// <summary>
        /// Creates a new game with a randomly generated lobby code.
        /// RESPONSE DATA = The lobby code of the new game created in the DB, or negative INT if error.
        /// </summary>
        /// <returns></returns>
        public Response<string> CreateGame()
        {
            //Create a game code and check against other DB game codes      
            Response<string> response = CreateGameCode();
            return response;
        }

        public Response<string> CreateGameCode()
        {
            GameDAL gameDAL = new GameDAL();
            Response<string> response = null;
            Boolean run = true;
            while (run)
            {
                response = gameDAL.CheckGameCode(GenerateCode());
                if (response.ErrorCode == ErrorCodes.EC_DATABASECONNECTERROR)
                {
                    run = false;
                    return response;
                }
                else if (response.ErrorCode == ErrorCodes.EC_ITEMALREADYEXISTS)
                {
                    run = true;
                }
                else
                {
                    run = false;
                    return response;
                    
                }
            }
            return response;
        }

        // Generates the actual game code
        public string GenerateCode()
        {
            //GENERATE CODE
            char[] chars = "abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            string gameCode = string.Empty;
            Random random = new Random();

            for (int i = 0; i < 6; i++)
            {
                int x = random.Next(1, chars.Length);
                //For avoiding repetition of Characters
                if (!gameCode.Contains(chars.GetValue(x).ToString()))
                    gameCode += chars.GetValue(x);
                else
                    i = i - 1;
            }

            return gameCode;
        }
    }
}
