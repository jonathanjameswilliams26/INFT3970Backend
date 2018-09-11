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
                gameBL.CreateJoinNotification(game.GameID, joinedPlayer.PlayerID);
            }

            //Loop through each of the players and update any player currently connected to the hub
            foreach (var player in response.Data)
            {
                //If the PlayerID is the playerID who joined skip this iteration
                if (player.PlayerID == playerID)
                    continue;

                //The player is connected to the hub, send live updates
                if(player.IsConnected)
                {
                    //If the game state is STARTING then the players are in the Lobby, update the lobby list
                    if(game.GameState == "STARTING")
                        await _hubContext.Clients.Client(player.ConnectionID).SendAsync("UpdateGameLobbyList");

                    //If the game state is PLAYING - Send a notification to the players in the game.
                    if(game.GameState == "PLAYING")            
                        await _hubContext.Clients.Client(player.ConnectionID).SendAsync("UpdateNotifications");
                }

                //Otherwise, the player is not connected to the Hub, send a notification via the contact information
                else
                {
                    //Don't send a notification when the game is in a STARTING state (in lobby)
                    //Send a notification when a new player joins when the game is currently playing.
                    if (game.GameState == "PLAYING")
                    {
                        //If the Player has an email address send the notification the email
                        if (!string.IsNullOrWhiteSpace(player.Email))
                            EmailSender.SendInBackground(player.Email, "New Player Joined Your Game", joinedPlayer.Nickname + " has joined your game of CamTag.", false);

                        //otherwise, send to the players phone
                        else
                            TextMessageSender.SendInBackground(joinedPlayer.Nickname + " has joined your game of CamTag.", player.Phone);
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






        /// <summary>
        /// Updates all the clients either via IN-GAME notifications or notificiations via text/email that
        /// a new photo has been uploaded to their game of CamTag and is ready to be voted on.
        /// </summary>
        /// <param name="uploadedPhoto">The photo model which has been uploaded.</param>
        public void UpdatePhotoUploaded(Photo uploadedPhoto)
        {
            //Get the list of players currently in the game
            PlayerDAL playerDAL = new PlayerDAL();
            string test = playerDAL.ToString();
            Response<List<Player>> response = playerDAL.GetGamePlayerList(uploadedPhoto.TakenByPlayerID, false);

            //If an error occurred while trying to get the list of players exit the method
            if (response.Type == "ERROR")
                return;

            //If the list of players is empty exit the method
            if (response.Data.Count == 0)
                return;


            //Loop through all the players in the game and send a Hub method or send a notification to their contact details.
            foreach(Player player in response.Data)
            {
                //If the playerID = the playerID who took the photo or is the playerID who the photo is of, skip this
                //iteration because they will not be voting on the photo and will not receive a notification
                if (player.PlayerID == uploadedPhoto.TakenByPlayerID || player.PlayerID == uploadedPhoto.PhotoOfPlayerID)
                    continue;

                //The player is connected to the Hub, call a live update method
                if(player.IsConnected)
                {
                    //TODO: Send out live update to client
                }

                //Otherwise, the player is not currently in the WebApp, send a notification to their contact details.
                else
                {
                    //If the sendToPlayer has an email address send the notification the email
                    if (!string.IsNullOrWhiteSpace(player.Email))
                        EmailSender.SendInBackground(player.Email, "New Photo Submitted", "A new photo has been uploaded in your game of CamTag. Click the link to cast your vote. LINK HERE...", false);

                    //otherwise, send to the players phone
                    else
                        TextMessageSender.SendInBackground("A new photo has been uploaded in your game of CamTag. Click the link to cast your vote. LINK HERE...", player.Phone);
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


            //TODO: Add notifications to DB


            //If the TakenByPlayer is connected to the Hub send out a live notification update
            if (photo.TakenByPlayer.IsConnected)
            {
                //TODO: Call live update hub method
            }
            //Otherwise, send a text message or email notification
            else
            {
                if (!string.IsNullOrWhiteSpace(photo.TakenByPlayer.Email))
                    EmailSender.SendInBackground(photo.TakenByPlayer.Email, "Voting Complete", takenByMsgTxt, false);
                else
                    TextMessageSender.SendInBackground(takenByMsgTxt, photo.TakenByPlayer.Phone);
            }


            //If the PhotoOfPlayer is connected to the Hub send out a live notification update
            if (photo.TakenByPlayer.IsConnected)
            {
                //TODO: Call live update hub method
            }
            //Otherwise, send a text message or email notification
            else
            {
                if (!string.IsNullOrWhiteSpace(photo.PhotoOfPlayer.Email))
                    EmailSender.SendInBackground(photo.PhotoOfPlayer.Email, "Voting Complete", photoOfMsgTxt, false);
                else
                    TextMessageSender.SendInBackground(photoOfMsgTxt, photo.PhotoOfPlayer.Phone);
            }
        }
    }
}
