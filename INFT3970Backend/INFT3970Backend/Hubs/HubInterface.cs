using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using INFT3970Backend.Business_Logic_Layer;
using INFT3970Backend.Models;
using INFT3970Backend.Data_Access_Layer;

namespace INFT3970Backend.Hubs
{
    public class HubInterface
    {
        private readonly IHubContext<ApplicationHub> _hubContext;

        public HubInterface(IHubContext<ApplicationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async void UpdatePlayerJoined(int playerID)
        {
            //Get all the players currently in the game
            PlayerBL playerBL = new PlayerBL();
            Response<List<Player>> response = playerBL.GetAllPlayersInGame(playerID);

            //If an error occurred while trying to get the list of players exit the method
            if (response.Type == "ERROR")
                return;

            //If the list of players is empty exit the method
            if (response.Data.Count == 0)
                return;

            

            

            //Loop through each of the players and update any player currently connected to the hub
            foreach(var player in response.Data)
            {
                if(!String.IsNullOrEmpty(player.ConnectionID))
                    await _hubContext.Clients.Client(player.ConnectionID).SendAsync("UpdateGameLobbyList");
            }
        }
    }
}
