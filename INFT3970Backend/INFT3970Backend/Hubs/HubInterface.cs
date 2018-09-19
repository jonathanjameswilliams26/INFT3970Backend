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
            //Get the list of players currently in the game
            GameDAL gameDAL = new GameDAL();
            Response<GamePlayerList> response = gameDAL.GetAllPlayersInGame(playerID, true, "INGAME", "AZ");

            //If an error occurred while trying to get the list of players exit the method
            if (!response.IsSuccessful())
                return;

            //Get the player who joined
            Player joinedPlayer = GetPlayerFromList(playerID, response.Data.Players);

            //Create notifications of new player joining, if the game has started
            GameBL gameBL = new GameBL();
            if (response.Data.Game.GameState == "PLAYING" || response.Data.Game.GameState == "STARTING")
                gameBL.CreateJoinNotification(joinedPlayer.PlayerID);

            //Loop through each of the players and update any player currently connected to the hub
            foreach (var player in response.Data.Players)
            {
                //If the PlayerID is the playerID who joined skip this iteration
                if (player.PlayerID == playerID)
                    continue;

                //The player is connected to the hub, send live updates
                if(player.IsConnected)
                {
                    //If the game state is STARTING then the players are in the Lobby, update the lobby list
                    if(response.Data.Game.GameState == "IN LOBBY")
                        await _hubContext.Clients.Client(player.ConnectionID).SendAsync("UpdateGameLobbyList");

                    //Send a notification to the players in the game.
                    else           
                        await _hubContext.Clients.Client(player.ConnectionID).SendAsync("UpdateNotifications");
                }

                //Otherwise, the player is not connected to the Hub, send a notification via the contact information
                else
                {
                    //Don't send a notification when the game is in a STARTING state (in lobby)
                    //Send a notification when a new player joins when the game is currently playing.
                    if (response.Data.Game.GameState == "PLAYING")
                    {
                        string message = joinedPlayer.Nickname + " has joined your game of CamTag.";
                        if (player.HasEmail())
                            EmailSender.SendInBackground(player.Email, "New Player Joined Your Game", message, false);
                        else
                            TextMessageSender.SendInBackground(message, player.Phone);
                    }
                }
            }
        }





        /// <summary>
        /// Gets a Player object from the List of players which has the specified PlayerID.
        /// NULL is returned if the specified playerID is not found within the list of players.
        /// </summary>
        /// <param name="playerID">The playerID searching for</param>
        /// <param name="list">The list of players to be searched.</param>
        /// <returns>The player object found matching the specified ID. NULL if not found.</returns>
        private Player GetPlayerFromList(int playerID, List<Player> list)
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






        /// <summary>
        /// Updates all the clients either via IN-GAME notifications or notificiations via text/email that
        /// a new photo has been uploaded to their game of CamTag and is ready to be voted on.
        /// </summary>
        /// <param name="uploadedPhoto">The photo model which has been uploaded.</param>
        public async void UpdatePhotoUploaded(Photo uploadedPhoto)
        {
            //Get the list of players currently in the game
            GameDAL gameDAL = new GameDAL();
            Response<GamePlayerList> response = gameDAL.GetAllPlayersInGame(uploadedPhoto.GameID, false, "INGAME", "AZ");

            //If an error occurred while trying to get the list of players exit the method
            if (!response.IsSuccessful())
                return;


            //Loop through all the players in the game and send a Hub method or send a notification to their contact details.
            foreach(Player player in response.Data.Players)
            {
                //If the playerID = the playerID who took the photo or is the playerID who the photo is of, skip this
                //iteration because they will not be voting on the photo and will not receive a notification
                if (player.PlayerID == uploadedPhoto.TakenByPlayerID || player.PlayerID == uploadedPhoto.PhotoOfPlayerID)
                    continue;

                //The player is connected to the Hub, call a live update method
                if(player.IsConnected)
                    await _hubContext.Clients.Client(player.ConnectionID).SendAsync("UpdatePhotoUploaded");

                //Otherwise, the player is not currently in the WebApp, send a notification to their contact details.
                else
                {
                    string message = "A new photo has been uploaded in your game of CamTag. Click the link to cast your vote. LINK HERE...";
                    if (player.HasEmail())
                        EmailSender.SendInBackground(player.Email, "New Photo Submitted", message, false);
                    else
                        TextMessageSender.SendInBackground(message, player.Phone);
                }
            }
        }







        public async void UpdatePhotoVotingCompleted(Photo photo)
        {
            //Generate the messages to be send
            string takenByMsgTxt = "";
            string photoOfMsgTxt = "";

            //If the yes votes are greater than the no votes the the photo is successful
            if(photo.NumYesVotes > photo.NumNoVotes)
            {
                takenByMsgTxt = "You have successfully tagged " + photo.PhotoOfPlayer.Nickname + ".";
                photoOfMsgTxt = "You have been tagged by " + photo.TakenByPlayer.Nickname + ".";
            }
            //Otherwise, the photo was not successful
            else
            {
                takenByMsgTxt = "You did not successfully tag " + photo.PhotoOfPlayer.Nickname + " because other players voted \"No\" on the photo you submitted.";
                photoOfMsgTxt = photo.TakenByPlayer.Nickname + " failed to tag you.";
            }

            
            //Add the notifications to the database
            GameDAL gameDAL = new GameDAL();
            gameDAL.CreateTagResultNotification(photo.TakenByPlayerID, photo.PhotoOfPlayerID, (photo.NumYesVotes > photo.NumNoVotes));


            //If the TakenByPlayer is connected to the Hub send out a live notification update
            if (photo.TakenByPlayer.IsConnected)
                await _hubContext.Clients.Client(photo.TakenByPlayer.ConnectionID).SendAsync("UpdateNotifications");
            
            //Otherwise, send a text message or email notification
            else
            {
                if (photo.TakenByPlayer.HasEmail())
                    EmailSender.SendInBackground(photo.TakenByPlayer.Email, "Voting Complete", takenByMsgTxt, false);
                else
                    TextMessageSender.SendInBackground(takenByMsgTxt, photo.TakenByPlayer.Phone);
            }


            //If the PhotoOfPlayer is connected to the Hub send out a live notification update
            if (photo.PhotoOfPlayer.IsConnected)
                await _hubContext.Clients.Client(photo.PhotoOfPlayer.ConnectionID).SendAsync("UpdateNotifications");

            //Otherwise, send a text message or email notification
            else
            {
                if (photo.PhotoOfPlayer.HasEmail())
                    EmailSender.SendInBackground(photo.PhotoOfPlayer.Email, "Voting Complete", photoOfMsgTxt, false);
                else
                    TextMessageSender.SendInBackground(photoOfMsgTxt, photo.PhotoOfPlayer.Phone);
            }
        }





        /// <summary>
        /// Sends a notification to the Players in the game that a player has left the game.
        /// If the game is currently in the Lobby (GameState = STARTING) then no notification will be sent, the lobby list will be updated.
        /// If the game is currently playing (GameState = PLAYING) then a notification will be sent to the players in the game.
        /// A notification will be sent to the players contact (Email or phone) if the player is not currently connected to the application hub.
        /// Otherwise, if the player is currently in the app and connected to the hub an InGame notification will be sent to the client.
        /// </summary>
        /// <param name="playerID">The playerID of the player who left the game</param>
        public async void UpdatePlayerLeft(int playerID)
        {
            //Get all the players currently in the game
            GameDAL gameDAL = new GameDAL();
            Response<GamePlayerList> response = gameDAL.GetAllPlayersInGame(playerID, true, "INGAMEALL", "AZ");

            //If an error occurred while trying to get the list of players exit the method
            if (!response.IsSuccessful())
                return;

            //Get the player who left
            Player leftPlayer = GetPlayerFromList(playerID, response.Data.Players);

            //Create notifications of new player joining, if the game has started
            GameBL gameBL = new GameBL();
            if (response.Data.Game.GameState == "PLAYING")
                gameBL.CreateLeaveNotification(playerID);

            //Loop through each of the players and update any player currently connected to the hub
            foreach (var player in response.Data.Players)
            {
                //If the PlayerID is the playerID who left or is a player who has left the game skip the iteration
                if (player.PlayerID == playerID || player.HasLeftGame)
                    continue;

                //The player is connected to the hub, send live updates
                if (player.IsConnected)
                {
                    //If the game state is STARTING then the players are in the Lobby, update the lobby list
                    if (response.Data.Game.GameState == "IN LOBBY")
                        await _hubContext.Clients.Client(player.ConnectionID).SendAsync("UpdateGameLobbyList");

                    //If the game state is PLAYING - Send a notification to the players in the game.
                    if (response.Data.Game.GameState == "PLAYING")
                        await _hubContext.Clients.Client(player.ConnectionID).SendAsync("UpdateNotifications");
                }

                //Otherwise, the player is not connected to the Hub, send a notification via the contact information
                else
                {
                    //Don't send a notification when the game is IN LOBBY state
                    //Send a notification when a new player joins when the game is currently playing.
                    if (response.Data.Game.GameState == "PLAYING")
                    {
                        string message = leftPlayer.Nickname + " has left your game of CamTag.";
                        if (player.HasEmail())
                            EmailSender.SendInBackground(player.Email, "A Player Left Your Game", message, false);
                        else
                            TextMessageSender.SendInBackground(message, player.Phone);
                    }
                }
            }
        }





        /// <summary>
        /// Sends updates to players once the game has been completed.
        /// Will send a notification to Phone/Email if the players arnt connected to the hub.
        /// If the players are connected to the hub a hub method will be invoked.
        /// </summary>
        /// <param name="gameID">The gameID which was completed</param>
        public async void UpdateGameCompleted(Game game)
        {
            //Get the list of players from the game
            GameDAL gameDAL = new GameDAL();
            Response<GamePlayerList> response = gameDAL.GetAllPlayersInGame(game.GameID, false, "ALL", "AZ");

            if (!response.IsSuccessful())
                return;

            //Loop through each of the players and send out the notifications
            foreach(var player in response.Data.Players)
            {
                //If the player left the game, not verified or is deleted skip the iteration
                if (player.HasLeftGame || !player.IsVerified || player.IsDeleted)
                    continue;

                //If the player is currently connected to the application send them an
                if(player.IsConnected)
                    await _hubContext.Clients.Client(player.ConnectionID).SendAsync("GameCompleted");

                //Otherwise, send them a text message or email letting them know the game is completed
                else
                {
                    string message = "Your game of CamTag has been completed. Thanks for playing.";
                    if (player.HasEmail())
                        EmailSender.SendInBackground(player.Email, "Game Completed", message, false);
                    else
                        TextMessageSender.SendInBackground(message, player.Phone);
                }
            }
        }






        /// <summary>
        /// Send an update to a player that their ammo has been replenished.
        /// An update will only be sent out when a player's ammo has went from 0 to 1.
        /// </summary>
        /// <param name="player">The player being updated.</param>
        public async void UpdateAmmoReplenished(Player player)
        {
            //Create an ammo notification
            GameBL gameBL = new GameBL();
            Response<object> response = gameBL.CreateAmmoNotification(player.PlayerID);

            //If the ammo notification was not successful leave the method
            if (!response.IsSuccessful())
                return;
            
            //If the player is connected to the hub call the UpdateNotifications and the AmmoReplenished client methods
            if (player.IsConnected)
            {
                await _hubContext.Clients.Client(player.ConnectionID).SendAsync("UpdateNotifications");
                await _hubContext.Clients.Client(player.ConnectionID).SendAsync("AmmoReplenished");
            }

            //Otherwise, send out a text message or email
            else
            {
                string message = "Your ammo has now been replenished, get back in the game.";
                if (player.HasEmail())
                    EmailSender.SendInBackground(player.Email, "Ammo Replenished", message, false);
                else
                    TextMessageSender.SendInBackground(message, player.Phone);
            }
        }





        /// <summary>
        /// Sends an update to players that the game is now in a PLAYING state, so the players
        /// can take photos etc. Will send updates via the Hub to connected clients or via
        /// email / text if the player is not connected.
        /// </summary>
        /// <param name="game">The Game which is now playing</param>
        public async void UpdateGameNowPlaying(Game game)
        {
            //Get the list of players from the game
            GameDAL gameDAL = new GameDAL();
            Response<GamePlayerList> response = gameDAL.GetAllPlayersInGame(game.GameID, false, "INGAME", "AZ");

            if (!response.IsSuccessful())
                return;

            //Loop through each of the players and invoke the Hub method or send out a notification that the game has started
            foreach(var player in response.Data.Players)
            {
                //If the player is connected invoke the hub method
                if (player.IsConnected)
                    await _hubContext.Clients.Client(player.ConnectionID).SendAsync("GameNowPlaying");

                //Otherwise, send an email/text message notification
                else
                {
                    string message = "Your game of CamTag is now playing. Go tag other players.";
                    if (player.HasEmail())
                        EmailSender.SendInBackground(player.Email, "Game Now Playing", message, false);
                    else
                        TextMessageSender.SendInBackground(message, player.Phone);
                }
            }
        }




        /// <summary>
        /// Sends an update to players that the game is now in a STARTING state, so the host
        /// player has clicked "begin game" and the Game will start in 10 mins. It will send
        /// updates to players via the Hub or email / text if the player is not connected.
        /// </summary>
        /// <param name="game">The Game which has now started.</param>
        public async void UpdateGameInStartingState(Game game)
        {
            //Get the list of players from the game
            GameDAL gameDAL = new GameDAL();
            Response<GamePlayerList> response = gameDAL.GetAllPlayersInGame(game.GameID, false, "INGAME", "AZ");

            if (!response.IsSuccessful())
                return;

            //Loop through each of the players and invoke the Hub method or send out a notification that the game will begin soon
            foreach (var player in response.Data.Players)
            {
                //If the player is connected invoke the hub method
                if (player.IsConnected)
                    await _hubContext.Clients.Client(player.ConnectionID).SendAsync("GameStarting");

                //Otherwise, send an email/text message notification
                else
                {
                    string message = "Your game of CamTag will begin at: " + game.StartTime.Value.ToString("dd/MM/yyyy HH:mm tt");

                    if (player.HasEmail())
                        EmailSender.SendInBackground(player.Email, "Game Starting Soon", message, false);
                    else
                        TextMessageSender.SendInBackground(message, player.Phone);
                }
            }
        }
    }
}
