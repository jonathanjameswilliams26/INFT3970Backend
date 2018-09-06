using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using INFT3970Backend.Business_Logic_Layer;
using INFT3970Backend.Models;
using INFT3970Backend.Data_Access_Layer;
using Hangfire;

namespace INFT3970Backend.Hubs
{
    public class HubInterface
    {
        private readonly IHubContext<ApplicationHub> _hubContext;

        public HubInterface(IHubContext<ApplicationHub> hubContext)
        {
            _hubContext = hubContext;
        }




        /// <summary>
        /// Sends a notification to the Players in the game that a new player is in the game.
        /// If the game is currently in the Lobby (GameState = STARTING) then no notification will be sent, the lobby list will be updated.
        /// If the game is currently playing (GameState = PLAYING) then a notification will be sent to the players in the game.
        /// A notification will be sent to the players contact (Email or phone) if the player is not currently connected to the application hub.
        /// Otherwise, if the player is currently in the app and connected to the hub an InGame notification will be sent to the client.
        /// </summary>
        /// <param name="playerID"></param>
        public async void UpdatePlayerJoined(int playerID)
        {
            //Get all the players currently in the game
            PlayerDAL playerDAL = new PlayerDAL();
            Response<List<Player>> response = playerDAL.GetGamePlayerList(playerID, true);

            //If an error occurred while trying to get the list of players exit the method
            if (response.Type == "ERROR")
                return;

            //If the list of players is empty exit the method
            if (response.Data.Count == 0)
                return;

            //Get the game the players are joined to
            Game game = response.Data[0].Game;

            //Get the player who joined
            Player joinedPlayer = GetJoinedPlayerFromList(playerID, response.Data);

            //Create notifications of new player joining, if the game has started
            GameBL gameBL = new GameBL();
            if (game.GameState == "PLAYING")
            {
                string gameJoin = "'" + joinedPlayer.Nickname + "' has joined the game.";
                gameBL.CreateNotification(gameJoin, "JOIN", game.GameID, joinedPlayer.PlayerID);
            }

            //Loop through each of the players and update any player currently connected to the hub
            foreach (var player in response.Data)
            {
                //If the PlayerID is the playerID who joined skip this iteration
                if (player.PlayerID == playerID)
                    continue;

                //The player is connected to the hub, send live updates
                if(!String.IsNullOrEmpty(player.ConnectionID))
                {
                    //If the game state is STARTING then the players are in the Lobby, update the lobby list
                    if(game.GameState == "STARTING")
                        await _hubContext.Clients.Client(player.ConnectionID).SendAsync("UpdateGameLobbyList");

                    //If the game state is PLAYING - Send a notification to the players in the game.
                    if(game.GameState == "PLAYING")
                    {                
                        await _hubContext.Clients.Client(player.ConnectionID).SendAsync("UpdateNotifications");
                    }
                }

                //Otherwise, the player is not connected to the Hub, send a notification via the contact information
                else
                {
                    //Don't send a notification when the game is in a STARTING state (in lobby)
                    //Send a notification when a new player joins when the game is currently playing.
                    if (game.GameState == "PLAYING")
                        SendToContactPlayerJoinNotification(player, joinedPlayer);
                }
            }
        }




        /// <summary>
        /// Sends a "Player Joined" notification to the sendToPlayer's contact details.
        /// Will send an email if avaliable, otherwise, will send a text message.
        /// The message will consist of "{joinedPlayer Nickname} joined your game of CamTag".
        /// </summary>
        /// <param name="sendToPlayer"></param>
        /// <param name="joinedPlayer"></param>
        private void SendToContactPlayerJoinNotification(Player sendToPlayer, Player joinedPlayer)
        {
            //If the sendToPlayer has an email address send the notification the email
            if(!string.IsNullOrWhiteSpace(sendToPlayer.Email))
            {
                BackgroundJob.Enqueue<EmailSender>(x => 
                    x.SendEmail(sendToPlayer.Email, 
                                "New Player Joined", 
                                joinedPlayer.Nickname + " joined your game of CamTag.", 
                                false));
            }
            //Othrwise, send it to their phone number
            else
            {
                BackgroundJob.Enqueue<TextMessageSender>(x =>
                    x.Send(joinedPlayer.Nickname + " joined your game of CamTag.", sendToPlayer.Phone));
            }
        }





        /// <summary>
        /// Gets a Player object from the List of players which has the specified PlayerID.
        /// NULL is returned if the specified playerID is not found within the list of players.
        /// </summary>
        /// <param name="playerID">The playerID searching for</param>
        /// <param name="list">The list of players to be searched.</param>
        /// <returns>The player object found matching the specified ID. NULL if not found.</returns>
        private Player GetJoinedPlayerFromList(int playerID, List<Player> list)
        {
            bool isFound = false;
            int i = 0;
            while(!isFound && i < list.Count)
            {
                if (list[i].PlayerID == playerID)
                    isFound = true;
                else
                    i++;
            }
            if (isFound)
                return list[i];
            else
                return null;
        }
    }
}
