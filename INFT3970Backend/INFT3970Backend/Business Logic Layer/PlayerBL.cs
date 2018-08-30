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
