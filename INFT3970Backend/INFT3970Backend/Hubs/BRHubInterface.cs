using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using INFT3970Backend.Models;
using INFT3970Backend.Data_Access_Layer;
using System;
using INFT3970Backend.Helpers;

namespace INFT3970Backend.Hubs
{
    public class BRHubInterface
    {
        private readonly IHubContext<ApplicationHub> _hubContext;
        public BRHubInterface(IHubContext<ApplicationHub> hubContext)
        {
            _hubContext = hubContext;
        }



        /// <summary>
        /// The client live update method which is used to update all the clients that a player has been
        /// eliminated from the game by taking too many photos outside of the zone.
        /// </summary>
        /// <param name="eliminatedPlayer"></param>
        public async void UpdatePlayerEliminated(BRPlayer eliminatedPlayer)
        {
            //Get the list of players currently in the game
            var gameDAL = new GameDAL();
            var gameResponse = gameDAL.GetAllPlayersInGame(eliminatedPlayer.PlayerID, true, "INGAME", "AZ");

            //If an error occurred while trying to get the list of players exit the method
            if (!gameResponse.IsSuccessful())
                return;

            //Loop through all the players in the game and send an update for notifications. The notidications have already been created
            foreach (var player in gameResponse.Data.Players)
            {
                if (player.PlayerID == eliminatedPlayer.PlayerID)
                    continue;

                //The player is connected to the Hub, call a live update method to update the notifications
                if (player.IsConnected)
                    await _hubContext.Clients.Client(player.ConnectionID).SendAsync("UpdateNotifications");
            }
        }
    }
}
