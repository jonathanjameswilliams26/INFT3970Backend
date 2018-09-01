using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using INFT3970Backend.Data_Access_Layer;
using INFT3970Backend.Models;

namespace INFT3970Backend.Business_Logic_Layer
{
    public class PlayerBL
    {
        /// <summary>
        /// Get the list of all players currently inside a game.
        /// </summary>
        /// <param name="playerID">The player ID. This playerID can be used to find what game they are in and get all other players</param>
        /// <returns>A list of all the players currently inside the game which the passed in playerID is in.</returns>
        public Response<List<Player>> GetAllPlayersInGame(int playerID)
        {
            //Call the DataAccessLayer to get the list of players in the same game from the database
            PlayerDAL playerDAL = new PlayerDAL();
            return playerDAL.GetGamePlayerList(playerID);
        }




        /// <summary>
        /// Updates the players connection ID
        /// </summary>
        /// <param name="playerID">The PlayerID being updated</param>
        /// <param name="connectionID">The new ConnectionID</param>
        /// <returns></returns>
        public Response<object> UpdateConnectionID(int playerID, string connectionID)
        {
            //Call the Data Access Layer to update the playerID's connectionID in the Database
            PlayerDAL playerDAL = new PlayerDAL();
            return playerDAL.UpdateConnectionID(playerID, connectionID);
        }
    }
}
