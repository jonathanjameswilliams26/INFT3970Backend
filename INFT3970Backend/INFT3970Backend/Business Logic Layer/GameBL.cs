using INFT3970Backend.Data_Access_Layer;
using INFT3970Backend.Models;
using System;

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
            GameDAL gameDAL = new GameDAL();
            Response<string> response = null;

            bool doRun = true;
            while (doRun)
            {
                //Call the data access layer to create a new game using the code passed in
                response = gameDAL.CreateGame(GenerateCode());

                //If the response is successful the game was successfully created
                if (response.Type == ResponseType.SUCCESS)
                    doRun = false;

                //If the response contains an error code of ITEMALREADYEXISTS then the game code is not unique,
                //Want to run again and generate a new code
                else if (response.ErrorCode == ErrorCodes.EC_ITEMALREADYEXISTS)
                    doRun = true;

                //If the response contains any other error code we do not want to run again because an un expected
                //error occurred and we want to return the error response.
                else
                    doRun = false;
            }
            return response;
        }




        /// <summary>
        /// Deactivates the game created by the host player because an unexpected error occurred while the host player tried to join the game
        /// </summary>
        /// <param name="gameCode">The game code to deactivate</param>
        public void DeactivateGameAfterHostJoinError(string gameCode)
        {
            new GameDAL().DeactivateGameAfterHostJoinError(gameCode);
        }

        

       
        /// <summary>
        /// Generates the game code for the game which players use to join the game.
        /// </summary>
        /// <returns>The randomly generated game code</returns>
        private string GenerateCode()
        {
            //GENERATE CODE
            char[] chars = "abcdefghijklmnopqrstuvwxyz1234567890".ToCharArray();
            string gameCode = string.Empty;
            Random random = new Random();

            for (int i = 0; i < 6; i++)
            {
                int x = random.Next(0, chars.Length);
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
