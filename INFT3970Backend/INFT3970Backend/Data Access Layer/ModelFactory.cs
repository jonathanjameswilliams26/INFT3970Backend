using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using INFT3970Backend.Models;

namespace INFT3970Backend.Data_Access_Layer
{
    public class ModelFactory
    {
        private SqlDataReader Reader;

        public ModelFactory(SqlDataReader reader)
        {
            Reader = reader;
        }

        private int GetColIndex(string colName)
        {
            try
            {
                return Reader.GetOrdinal(colName);
            }
            catch
            {
                return -1;
            }
        }

        private int GetInt(string colName)
        {
            return Reader.GetInt32(GetColIndex(colName));
        }


        private bool GetBool(string colName)
        {
            return Reader.GetBoolean(GetColIndex(colName));
        }


        private string SafeGetString(string colName)
        {
            int index = GetColIndex(colName);
            if (!Reader.IsDBNull(index))
                return Reader.GetString(index);
            return string.Empty;
        }

        private double GetDouble(string colName)
        {
            return Reader.GetDouble(GetColIndex(colName));
        }

        private DateTime GetDateTime(string colName)
        {
            return Reader.GetDateTime(GetColIndex(colName));
        }


        public Notification NotificationFactory()
        {
            try
            {
                Notification notification = new Notification();
                notification.NotificationID = GetInt("NotificationID");
                notification.MessageText = SafeGetString("MessageText");
                notification.Type = SafeGetString("NotificationType");
                notification.IsRead = GetBool("IsRead");
                notification.IsActive = GetBool("NotificationIsActive");
                notification.GameID = GetInt("GameID");
                notification.PlayerID = GetInt("PlayerID");
                return notification;
            }
            catch
            {
                return null;
            }
        }


        public Player PlayerFactory(bool doGetGame)
        {
            try
            {
                Player player = new Player();
                Game game = null;
                player.PlayerID = GetInt("PlayerID");
                player.Nickname = SafeGetString("Nickname");
                player.Phone = SafeGetString("Phone");
                player.Email = SafeGetString("Email");
                player.SelfieFilePath = SafeGetString("SelfieDataURL");
                player.NumKills = GetInt("NumKills");
                player.NumDeaths = GetInt("NumDeaths");
                player.NumPhotosTaken = GetInt("NumPhotosTaken");
                player.IsHost = GetBool("IsHost");
                player.IsVerified = GetBool("IsVerified");
                player.ConnectionID = SafeGetString("ConnectionID");
                player.IsConnected = GetBool("IsConnected");
                player.IsActive = GetBool("PlayerIsActive");

                //Build the Game object for the player
                if(doGetGame)
                {
                    game = GameFactory();
                    player.Game = game;
                    
                }
                return player;
            }
            catch
            {
                return null;
            }
        }


        public Photo PhotoFactory(bool doGetGame, bool doGetPlayerWhoTookPhoto, bool doGetPlayerWhoPhotoIsOf)
        {
            try
            {
                //Build the Photo Object
                Photo photo = new Photo();
                photo.PhotoID = GetInt("PhotoID");
                photo.Lat = GetDouble("Lat");        
                photo.Long = GetDouble("Long");
                photo.PhotoDataURL = SafeGetString("PhotoDataURL");
                photo.TimeTaken = GetDateTime("TimeTaken");
                photo.VotingFinishTime = GetDateTime("VotingFinishTime");
                photo.NumYesVotes = GetInt("NumYesVotes");
                photo.NumNoVotes = GetInt("NumNoVotes");
                photo.IsVotingComplete = GetBool("IsVotingComplete");
                photo.IsActive = GetBool("PhotoIsActive");
                photo.GameID = GetInt("GameID");
                photo.TakenByPlayerID = GetInt("TakenByPlayerID");
                photo.PhotoOfPlayerID = GetInt("PhotoOfPlayerID");

                //Build the game object if you need to get the game
                if (doGetGame)
                    photo.Game = GameFactory();

                //Build the taken by player object
                if(doGetPlayerWhoTookPhoto)
                {
                    Player player = new Player();
                    player.PlayerID = photo.TakenByPlayerID;
                    player.Nickname = SafeGetString("TakenByPlayerNickname");
                    player.Phone = SafeGetString("TakenByPlayerPhone");
                    player.Email = SafeGetString("TakenByPlayerEmail");
                    player.SelfieFilePath = SafeGetString("TakenByPlayerSelfieDataURL");
                    player.NumKills = GetInt("TakenByPlayerNumKills");
                    player.NumDeaths = GetInt("TakenByPlayerNumDeaths");
                    player.NumPhotosTaken = GetInt("TakenByPlayerNumPhotosTaken");
                    player.IsHost = GetBool("TakenByPlayerIsHost");
                    player.IsVerified = GetBool("TakenByPlayerIsVerified");
                    player.ConnectionID = SafeGetString("TakenByPlayerConnectionID");
                    player.IsConnected = GetBool("TakenByPlayerIsConnected");
                    player.IsActive = GetBool("TakenByPlayerIsActive");
                    player.Game = photo.Game;
                    photo.TakenByPlayer = player;
                }


                //Build the photo of player object
                if (doGetPlayerWhoTookPhoto)
                {
                    Player player = new Player();
                    player.PlayerID = photo.PhotoOfPlayerID;
                    player.Nickname = SafeGetString("PhotoOfPlayerNickname");
                    player.Phone = SafeGetString("PhotoOfPlayerPhone");
                    player.Email = SafeGetString("PhotoOfPlayerEmail");
                    player.SelfieFilePath = SafeGetString("PhotoOfPlayerSelfieDataURL");
                    player.NumKills = GetInt("PhotoOfPlayerNumKills");
                    player.NumDeaths = GetInt("PhotoOfPlayerNumDeaths");
                    player.NumPhotosTaken = GetInt("PhotoOfPlayerNumPhotosTaken");
                    player.IsHost = GetBool("PhotoOfPlayerIsHost");
                    player.IsVerified = GetBool("PhotoOfPlayerIsVerified");
                    player.ConnectionID = SafeGetString("PhotoOfPlayerConnectionID");
                    player.IsConnected = GetBool("PhotoOfPlayerIsConnected");
                    player.IsActive = GetBool("PhotoOfPlayerIsActive");
                    player.Game = photo.Game;
                    photo.PhotoOfPlayer = player;
                }

                return photo;
            }
            catch
            {
                return null;
            }
        }


        public Game GameFactory()
        {
            try
            {
                Game game = new Game();
                game.GameID = GetInt("GameID");
                game.GameCode = SafeGetString("GameCode");
                game.NumOfPlayers = GetInt("NumOfPlayers");
                game.GameMode = SafeGetString("GameMode");
                game.StartTime = GetDateTime("StartTime");
                game.EndTime = GetDateTime("EndTime");
                game.GameState = SafeGetString("GameState");
                game.IsJoinableAtAnytime = GetBool("IsJoinableAtAnytime");
                game.IsActive = GetBool("GameIsActive");
                return game;
            }
            catch
            {
                return null;
            }
        }
    }
}
    

