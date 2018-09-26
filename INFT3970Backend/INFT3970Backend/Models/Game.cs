using INFT3970Backend.Models.Errors;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace INFT3970Backend.Models
{
    public class Game
    {
        //Private backing stores of public properites which have business logic behind them.
        private int gameID;
        private string gameCode;
        private string gameMode;
        private string gameState;
        private int numOfPlayers;



        public int GameID
        {
            get { return gameID; }
            set
            {
                var errorMSG = "GameID is invalid. Must be atleast 100000.";

                if (value == -1 || value >= 100000)
                    gameID = value;
                else
                    throw new InvalidModelException(errorMSG, ErrorCodes.MODELINVALID_GAME);
            }
        }



        public string GameCode
        {
            get { return gameCode; }
            set
            {
                var errorMSG = "GameCode is invalid. Must be 6 characters long and only contain letters and numbers.";

                if (value == null)
                    throw new InvalidModelException(errorMSG, ErrorCodes.MODELINVALID_GAME);

                var gameCodeRegex = new Regex(@"^[a-zA-Z0-9]{6,6}$");
                if (!gameCodeRegex.IsMatch(value))
                    throw new InvalidModelException(errorMSG, ErrorCodes.MODELINVALID_GAME);
                else
                    gameCode = value;
            }
        }



        public int NumOfPlayers
        {
            get { return numOfPlayers; }
            set
            {
                var errorMSG = "Number of players is invalid, cannot be less than 0 or greater than 16.";

                if (value < 0 || value > 16)
                    throw new InvalidModelException(errorMSG, ErrorCodes.MODELINVALID_GAME);
                else
                    numOfPlayers = value;
            }
        }



        public string GameMode
        {
            get { return gameMode; }
            set
            {
                var errorMSG = "Game mode is invalid.";

                if (value == null)
                    throw new InvalidModelException(errorMSG, ErrorCodes.MODELINVALID_GAME);

                if (value == "CORE")
                    gameMode = value;
                else
                    throw new InvalidModelException(errorMSG, ErrorCodes.MODELINVALID_GAME);
            }
        }



        public string GameState
        {
            get { return gameState; }
            set
            {
                var errorMSG = "Game state is invalid.";

                if (value == null)
                    throw new InvalidModelException(errorMSG, ErrorCodes.MODELINVALID_GAME);

                if (value == "IN LOBBY" || value == "STARTING" || value == "PLAYING" || value == "COMPLETED")
                    gameState = value;
                else
                    throw new InvalidModelException(errorMSG, ErrorCodes.MODELINVALID_GAME);
            }
        }




        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsJoinableAtAnytime { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public List<Player> Players { get; set; }





        /// <summary>
        /// Creates a game with default values
        /// </summary>
        public Game()
        {
            GameID = -1;
            GameCode = "abc123";
            GameMode = "CORE";
            GameState = "IN LOBBY";
        }



        /// <summary>
        /// Creates a game with default values and sets the GameCode
        /// </summary>
        /// <param name="gameCode">The 6 digit game code of the game.</param>
        public Game(string gameCode) : this()
        {
            GameCode = gameCode;
        }



        /// <summary>
        /// Creates a game with default values and set the GameID
        /// </summary>
        /// <param name="gameID">The ID of the game</param>
        public Game(int gameID) : this()
        {
            GameID = gameID;
        }




        /// <summary>
        /// Generates a 6 digit alphanumeric code which is the game code for the game.
        /// </summary>
        /// <returns>6 digit alphanumeric code</returns>
        public static string GenerateGameCode()
        {
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



        /// <summary>
        /// Check if the game is in a PLAYING state
        /// </summary>
        /// <returns></returns>
        public bool IsPlaying()
        {
            return GameState == "PLAYING";
        }


        /// <summary>
        /// Check if the game is in a STARTING state
        /// </summary>
        public bool IsStarting()
        {
            return GameState == "STARTING";
        }


        /// <summary>
        /// Check if the game is in a IN LOBBY state
        /// </summary>
        public bool IsInLobby()
        {
            return GameState == "IN LOBBY";
        }


        /// <summary>
        /// Check if the game is in a COMPLETED state
        /// </summary>
        public bool IsCompleted()
        {
            return GameState == "COMPLETED";
        }
    }
}
