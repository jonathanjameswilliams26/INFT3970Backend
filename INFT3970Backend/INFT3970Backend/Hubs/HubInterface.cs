using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using INFT3970Backend.Models;
using INFT3970Backend.Data_Access_Layer;
using System;

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
        /// Sends a notification to the players in the game that a new player has joined the game.
        /// 
        /// If the game is currently in the lobby or starting (GameState = IN LOBBY, STARTING) 
        /// then no notification will be sent, the lobby list will be updated.
        /// 
        /// If the game is currently playing (GameState = PLAYING) then a in game notification will be sent to 
        /// the players connect to the hub / have the web app open.
        /// 
        /// An out of game notification will be sent to the players contact (Email or phone) if the player is 
        /// not currently connected to the application hub when GameState = STARTING, PLAYING.
        /// </summary>
        /// <param name="joinedPlayer"></param>
        public async void UpdatePlayerJoinedGame(Player joinedPlayer)
        {
            //Get the list of players currently in the game
            var gameDAL = new GameDAL();
            var gameResponse = gameDAL.GetAllPlayersInGame(joinedPlayer.PlayerID, true, "INGAME", "AZ");

            //If an error occurred while trying to get the list of players exit the method
            if (!gameResponse.IsSuccessful())
                return;

            var game = gameResponse.Data;

            //Create notifications of new player joining, if the game is playing and the player joining is verified
            if (game.IsPlaying() && joinedPlayer.IsVerified)
                gameDAL.CreateJoinNotification(joinedPlayer);


            //Loop through each of the players and update any player currently in the game.
            foreach (var player in game.Players)
            {
                //If the PlayerID is the playerID who joined skip this iteration
                if (player.PlayerID == joinedPlayer.PlayerID)
                    continue;

                //Update players in game when connected to the hub / have the web app open
                if(player.IsConnected)
                {
                    //If the game is IN LOBBY or STARTING update the lobby list for both verfied and unverified players joining
                    if(game.IsInLobby() || game.IsStarting())
                        await _hubContext.Clients.Client(player.ConnectionID).SendAsync("UpdateGameLobbyList");

                    //Otherwise, update the players notifications and the scoreboard because they are currently playing the game
                    else if(game.IsPlaying() && joinedPlayer.IsVerified)
                    {
                        await _hubContext.Clients.Client(player.ConnectionID).SendAsync("UpdateNotifications");
                        await _hubContext.Clients.Client(player.ConnectionID).SendAsync("UpdateScoreboard");
                    }
                }
                //Otherwise, the player is out of the app.
                else
                {
                    //Only send a notification when the game is playing or starting
                    var message = joinedPlayer.Nickname + " has joined your game of CamTag.";
                    var subject = "New Player Joined Your Game";
                    if ((game.IsPlaying() || game.IsStarting()) && joinedPlayer.IsVerified)
                        player.ReceiveMessage(message, subject);
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
            var response = new GameDAL().GetAllPlayersInGame(uploadedPhoto.GameID, false, "INGAME", "AZ");

            //If an error occurred while trying to get the list of players exit the method
            if (!response.IsSuccessful())
                return;


            //Loop through all the players in the game and send a Hub method or send a notification to their contact details.
            foreach(var player in response.Data.Players)
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
                    string message = "A new photo has been uploaded in your game of CamTag. Cast your vote before time runs out!";
                    var subject = "New Photo Submitted";
                    player.ReceiveMessage(message, subject);
                }
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
        public async void UpdatePlayerLeftGame(Player leftPlayer)
        {
            //Get all the players currently in the game
            GameDAL gameDAL = new GameDAL();
            var gameResponse = gameDAL.GetAllPlayersInGame(leftPlayer.GameID, false, "INGAME", "AZ");

            //If an error occurred while trying to get the list of players exit the method
            if (!gameResponse.IsSuccessful())
                return;

            var game = gameResponse.Data;

            //Create notifications of player leaving, if the game is playing
            if (game.IsPlaying())
                gameDAL.CreateLeaveNotification(leftPlayer);

            //Loop through each of the players and update any player currently connected to the hub
            foreach (var player in game.Players)
            {
                //The player is connected to the hub, send live updates
                if (player.IsConnected)
                {
                    //If the game state is IN LOBBY or STARTING then the players are in the Lobby, update the lobby list
                    if (game.IsInLobby() || game.IsStarting())
                        await _hubContext.Clients.Client(player.ConnectionID).SendAsync("UpdateGameLobbyList");

                    //If the game state is PLAYING - Send a notification to the players in the game.
                    else
                    {
                        await _hubContext.Clients.Client(player.ConnectionID).SendAsync("UpdateNotifications");
                        await _hubContext.Clients.Client(player.ConnectionID).SendAsync("UpdateScoreboard");
                    }
                }

                //Otherwise, the player is not connected to the Hub, send a notification via the contact information
                else
                {
                    //Don't send a notification when the game is IN LOBBY or STARTING state
                    var message = leftPlayer.Nickname + " has left your game of CamTag.";
                    var subject = "A Player Left Your Game";
                    if (game.IsPlaying() || game.IsStarting())
                        player.ReceiveMessage(message, subject);
                }
            }
        }

        





        /// <summary>
        /// Sends updates to players once the game has been completed.
        /// Will send a notification to Phone/Email if the players arnt connected to the hub.
        /// If the players are connected to the hub a hub method will be invoked.
        /// </summary>
        /// <param name="gameID">The gameID which was completed</param>
        public async void UpdateGameCompleted(Game game, bool isNotEnoughPlayers)
        {
            //Get the list of players from the game
            var response = new GameDAL().GetAllPlayersInGame(game.GameID, false, "ALL", "AZ");

            if (!response.IsSuccessful())
                return;

            //Loop through each of the players and send out the notifications
            foreach(var player in response.Data.Players)
            {
                //Since the game is now completed needed to get all players. 
                //If they have left the game, have not been verified or deleted do not send them a notification
                if (player.HasLeftGame || !player.IsVerified || player.IsDeleted)
                    continue;

                //If the player is currently connected to the application send them a live update
                if(player.IsConnected)
                    await _hubContext.Clients.Client(player.ConnectionID).SendAsync("GameCompleted");

                //Otherwise, send them a text message or email letting them know the game is completed
                else
                {
                    var message = "Your game of CamTag has been completed. Thanks for playing!";
                    var subject = "Game Completed";
                    if (isNotEnoughPlayers)
                        message = "Your game of CamTag has been completed because there is no longer enough players in your game. Thanks for playing!";
                    player.ReceiveMessage(message, subject);
                }
            }
        }




        public async void UpdateLobbyEnded(Game game)
        {
            //Get the list of players from the game
            var response = new GameDAL().GetAllPlayersInGame(game.GameID, false, "ALL", "AZ");
            if (!response.IsSuccessful())
                return;

            //Loop through each of the players and send out the live update
            foreach(var player in response.Data.Players)
            {
                if(player.IsConnected)
                    await _hubContext.Clients.Client(player.ConnectionID).SendAsync("LobbyEnded");
            }
        }






        /// <summary>
        /// Send an update to a player that their ammo has been replenished.
        /// A text/mail notification will only be sent out when a player's ammo has gone from 0 to 1.
        /// </summary>
        /// <param name="player">The player being updated.</param>
        public async void UpdateAmmoReplenished(Player player)
        {
            //If the players ammo has gone from 0-1 create a notification for the player
            if (player.AmmoCount == 1)
            {
                //Create an ammo notification
                var response = new GameDAL().CreateAmmoNotification(player);

                //If the ammo notification was not successful leave the method
                if (!response.IsSuccessful())
                    return;
            }
                
            
            //If the player is connected to the hub call the UpdateNotifications and the AmmoReplenished client methods
            if (player.IsConnected)
            {
                //To indcate to the client that the ammo has increased to update the UI
                await _hubContext.Clients.Client(player.ConnectionID).SendAsync("AmmoReplenished");

                //If the ammo count is now 1, there will be an ammo notificaiton
                if (player.AmmoCount == 1)
                    await _hubContext.Clients.Client(player.ConnectionID).SendAsync("UpdateNotifications");
            }

            //Otherwise if ammo has gone from empty to not, send out a text message or email
            else if (player.AmmoCount == 1)
            {
                string message = "Your ammo has now been replenished, go get em!";
                var subject = "Ammo Replenished";
                player.ReceiveMessage(message, subject);
            }
        }

        public async void UpdateNotificationsRead(int playerID)
        {
            //Get the player from the database
            var response = new PlayerDAL().GetPlayerByID(playerID);
            if (!response.IsSuccessful())
                return;

            //If the player is connected to the hub update the notifications list
            if(response.Data.IsConnected)
                await _hubContext.Clients.Client(response.Data.ConnectionID).SendAsync("UpdateNotifications");
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
            var response = new GameDAL().GetAllPlayersInGame(game.GameID, false, "INGAME", "AZ");

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
                    var message = "Your game of CamTag is now playing. Go tag other players!";
                    var subject = "Game Now Playing";
                    player.ReceiveMessage(message, subject);
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
            var response = new GameDAL().GetAllPlayersInGame(game.GameID, false, "INGAME", "AZ");

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
                    var message = "Your game of CamTag will begin at: " + game.StartTime.Value.ToString("dd/MM/yyyy HH:mm:ss tt");
                    var subject = "Game Starting Soon";
                    player.ReceiveMessage(message, subject);
                }
            }
        }









        //BATTLE ROYALE HUB METHODS
        public async void BR_UpdatePlayerDisabled(Player player, int totalMinutesDisabled)
        {
            //Update the player if they are connected to the application
            if (player.IsConnected)
            {
                await _hubContext.Clients.Client(player.ConnectionID).SendAsync("UpdateNotifications");
                await _hubContext.Clients.Client(player.ConnectionID).SendAsync("PlayerDisabled", totalMinutesDisabled);
            }
            else
                player.ReceiveMessage("You have been disabled for " + totalMinutesDisabled + " minutes for taking a photo outside of the zone.", "You Have Been Disabled");
        }



        public async void BR_UpdatePlayerReEnabled(Player player)
        {
            //Get the player from the database as this will run after a certain amount of time.
            //Get the most updated player information
            var response = new PlayerDAL().GetPlayerByID(player.PlayerID);
            if (!response.IsSuccessful())
                return;

            player = response.Data;

            //Update the player if they are connected to the application
            if (player.IsConnected)
            {
                await _hubContext.Clients.Client(player.ConnectionID).SendAsync("UpdateNotifications");
                await _hubContext.Clients.Client(player.ConnectionID).SendAsync("PlayerReEnabled");
            }
            else
                player.ReceiveMessage("You have been re-enabled, go get em!", "You Have Been Re-enabled");
        }



        public async void UpdatePhotoVotingCompleted(Photo photo)
        {  
            //Generate the messages to be sent
            var takenByMsgTxt = "";
            var photoOfMsgTxt = "";
            var subject = "Voting Complete";

            GenerateVotingCompleteMessages(photo, ref takenByMsgTxt, ref photoOfMsgTxt);

            //Generate the notifications
            var isBR = photo.Game.IsBR();
            if (isBR)
                new GameDAL().CreateTagResultNotification(photo, true);
            else
                new GameDAL().CreateTagResultNotification(photo, false);

            //If the TakenByPlayer is not connected to the application send an out of game notification
            if (!photo.TakenByPlayer.IsConnected)
                photo.TakenByPlayer.ReceiveMessage(takenByMsgTxt, subject);

            //If the PhotoOfPlayer is not connected to the application send an out of game notification
            if (!photo.PhotoOfPlayer.IsConnected)
                photo.PhotoOfPlayer.ReceiveMessage(photoOfMsgTxt, subject);

            //Live update all other clients connected to the application
            //Get the list of players from the game
            var response = new GameDAL().GetAllPlayersInGame(photo.GameID, false, "INGAME", "AZ");
            if (!response.IsSuccessful())
                return;

            //Loop through each player and send the live in game update to update the notifications
            foreach(var player in response.Data.Players)
            {
                //If the player is not connected then continue
                if (!player.IsConnected)
                    continue;

                //If the player is the PhotoOfPlayer and the Photo was successful and it is a BR game, send out eliminated updated
                if(player.PlayerID == photo.PhotoOfPlayerID && photo.IsSuccessful && isBR)
                    await _hubContext.Clients.Client(player.ConnectionID).SendAsync("PlayerEliminated");

                //Otherwise, just update the notifications
                else
                    await _hubContext.Clients.Client(player.ConnectionID).SendAsync("UpdateNotifications");

                //Update the scoreboard
                await _hubContext.Clients.Client(player.ConnectionID).SendAsync("UpdateScoreboard");
            }
        }



        private void GenerateVotingCompleteMessages(Photo completedPhoto, ref string takenByMessage, ref string photoOfMessage)
        {
            //If the completed photo is apart of a BR game process the BR Eliminated message
            if(completedPhoto.Game.IsBR())
            {
                //If the photo was successful set the eliminated message
                if(completedPhoto.IsSuccessful)
                {
                    takenByMessage = "You have eliminated " + completedPhoto.PhotoOfPlayer.Nickname + ".";
                    photoOfMessage = "You have been eliminated by " + completedPhoto.TakenByPlayer.Nickname + ".";
                }
                //Otherwise, set the unsuccessful photo
                else
                {
                    takenByMessage = "You did not successfully tag " + completedPhoto.PhotoOfPlayer.Nickname + " because other players voted \"No\" on the photo you submitted.";
                    photoOfMessage = completedPhoto.TakenByPlayer.Nickname + " failed to tag you.";
                }
            }

            //Otherwise, the game is apart of a normal CORE game, process the normal messages
            else
            {
                //If the yes votes are greater than the no votes the the photo is successful
                if (completedPhoto.IsSuccessful)
                {
                    takenByMessage = "You have successfully tagged " + completedPhoto.PhotoOfPlayer.Nickname + ".";
                    photoOfMessage = "You have been tagged by " + completedPhoto.TakenByPlayer.Nickname + ".";
                }
                //Otherwise, the photo was not successful
                else
                {
                    takenByMessage = "You did not successfully tag " + completedPhoto.PhotoOfPlayer.Nickname + " because other players voted \"No\" on the photo you submitted.";
                    photoOfMessage = completedPhoto.TakenByPlayer.Nickname + " failed to tag you.";
                }
            }
        }
    }
}
