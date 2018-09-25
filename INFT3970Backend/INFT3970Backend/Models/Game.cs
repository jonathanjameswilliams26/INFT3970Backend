using INFT3970Backend.Models.Errors;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace INFT3970Backend.Models
{
    public class Game
    {

        private int gameID;
        private string gameCode;
        private string gameMode;
        private string gameState;
        private int numOfPlayers;
        private DateTime? startTime;
        private DateTime? endTime;



        public int GameID
        {
            get { return gameID; }
            set
            {
                if (value == -1 || value >= 100000)
                    gameID = value;

                else
                    throw new InvalidModelException(ErrorMessages.EM_GAME_ID, ErrorCodes.EC_PLAYER_ID);
            }
        }



        public string GameCode
        {
            get { return gameCode; }
            set
            {
                if (value == "empty")
                {
                    gameCode = value;
                    return;
                }

                if (string.IsNullOrWhiteSpace(value))
                    throw new InvalidModelException(ErrorMessages.EM_GAME_CODE, ErrorCodes.EC_GAME_CODE);

                var gameCodeRegex = new Regex(@"^[a-zA-Z0-9]{6,6}$");
                if (!gameCodeRegex.IsMatch(value))
                    throw new InvalidModelException(ErrorMessages.EM_GAME_CODE, ErrorCodes.EC_GAME_CODE);
                else
                    gameCode = value;
            }
        }



        public int NumOfPlayers
        {
            get { return numOfPlayers; }
            set
            {
                if (value < 0 || value > 16)
                    throw new InvalidModelException(ErrorMessages.EM_GAME_PLAYERCOUNT, ErrorCodes.EC_GAME_PLAYERCOUNT);
                else
                    numOfPlayers = value;
            }
        }



        public string GameMode
        {
            get { return gameMode; }
            set
            {
                if (value == "empty")
                {
                    gameMode = value;
                    return;
                }

                if (string.IsNullOrWhiteSpace(value))
                    throw new InvalidModelException(ErrorMessages.EM_GAME_MODE, ErrorCodes.EC_GAME_MODE);

                if (value == "CORE")
                    gameMode = value;
                else
                    throw new InvalidModelException(ErrorMessages.EM_GAME_MODE, ErrorCodes.EC_GAME_MODE);
            }
        }



        public DateTime? StartTime
        {
            get { return startTime; }
            set
            {
                if (value == null)
                    startTime = null;

                else if (EndTime.Value == null)
                    startTime = value;
                
                //If an end time exists, confirm the start time is less than
                else
                {
                    if (value.Value > EndTime.Value)
                        throw new InvalidModelException(ErrorMessages.EM_GAME_DATES, ErrorCodes.EC_GAME_DATES);
                    else
                        endTime = value;
                }
            }
        }



        public DateTime? EndTime
        {
            get { return endTime; }
            set
            {
                if (value == null)
                    endTime = null;

                else if (StartTime.Value == null)
                    endTime = value;

                //If a start time exists, confirm the end time is greater
                else
                {
                    if (startTime.Value > value.Value)
                        throw new InvalidModelException(ErrorMessages.EM_GAME_DATES, ErrorCodes.EC_GAME_DATES);
                    else
                        endTime = value;
                }
            }
        }



        public string GameState
        {
            get { return gameState; }
            set
            {
                if (value == "empty")
                {
                    gameState = value;
                    return;
                }

                if (string.IsNullOrWhiteSpace(value))
                    throw new InvalidModelException(ErrorMessages.EM_GAME_CODE, ErrorCodes.EC_GAME_CODE);

                if (value == "IN LOBBY" || value == "STARTING" || value == "PLAYING" || value == "COMPLETED")
                    gameState = value;
                else
                    throw new InvalidModelException(ErrorMessages.EM_GAME_STATE, ErrorCodes.EC_GAME_STATE);
            }
        }



        public bool IsJoinableAtAnytime { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public List<Player> Players { get; set; }

        public Game()
        {
            GameID = -1;
            GameCode = "empty";
            GameMode = "empty";
            GameState = "empty";
        }

        public Game(string gameCode)
        {
            GameID = -1;
            GameMode = "empty";
            GameState = "empty";
            GameCode = gameCode;
        }

        public Game(int gameID)
        {
            GameID = gameID;
            GameMode = "empty";
            GameState = "empty";
            GameCode = "empty";
        }
    }
}
