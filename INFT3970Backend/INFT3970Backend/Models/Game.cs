using INFT3970Backend.Models.Errors;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Text.RegularExpressions;

namespace INFT3970Backend.Models
{
    public class Game
    {
        //Minutes / Days in milliseconds, used for the timers
        const int ONE_MINUTE_MILLISECONDS = 60000;
        const int TEN_MINUTES_MILLISECONDS = 600000;
        const int ONE_HOUR_MILLISECONDS = 3600000;
        const int ONE_DAY_MILLISECONDS = 86400000;


        //Private backing stores of public properites which have business logic behind them.
        private int gameID;
        private string gameCode;
        private string gameMode;
        private string gameState;
        private int numOfPlayers;
        private int timeLimit;
        private int ammoLimit;
        private int replenishAmmoDelay;
        private int startDelay;
        private double longitude;
        private double latitude;
        private int radius;

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

                var uppercaseValue = value.ToUpper();

                if (uppercaseValue == "CORE" || uppercaseValue == "BR")
                    gameMode = uppercaseValue;
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

                var uppercaseValue = value.ToUpper();

                if (uppercaseValue == "IN LOBBY" || uppercaseValue == "STARTING" || uppercaseValue == "PLAYING" || uppercaseValue == "COMPLETED")
                    gameState = uppercaseValue;
                else
                    throw new InvalidModelException(errorMSG, ErrorCodes.MODELINVALID_GAME);
            }
        }





        public int TimeLimit
        {
            get { return timeLimit; }
            set
            {
                var errorMSG = "Time limit is invalid, must be between 10 minutes and 24 hours.";

                //The time limit of the game can only be between 10 minutes and one day
                if (value < TEN_MINUTES_MILLISECONDS || value > ONE_DAY_MILLISECONDS)
                    throw new InvalidModelException(errorMSG, ErrorCodes.MODELINVALID_GAME);
                else
                    timeLimit = value;
            }
        }




        public int AmmoLimit
        {
            get { return ammoLimit; }
            set
            {
                var errorMSG = "Ammo limit is invalid. Must be between 1 and 9.";

                //confirm the ammo limit passed in is within the valid range
                if (value < 1 || value > 9)
                    throw new InvalidModelException(errorMSG, ErrorCodes.MODELINVALID_GAME);
                else
                    ammoLimit = value;
            }
        }






        public int ReplenishAmmoDelay
        {
            get { return replenishAmmoDelay; }
            set
            {
                var errorMSG = "Reload ammo timer is invalid. Must be between 1 minute and 1 hour.";

                //confirm the replenish ammo deley passed in is within the valid range
                //The valid range is between 1 minute and 1 hour
                if (value < ONE_MINUTE_MILLISECONDS || value > ONE_HOUR_MILLISECONDS)
                    throw new InvalidModelException(errorMSG, ErrorCodes.MODELINVALID_GAME);
                else
                    replenishAmmoDelay = value;
            }
        }




        public int StartDelay
        {
            get { return startDelay; }
            set
            {
                var errorMSG = "Start delay is invalid. Must be between 1 minute and 10 minutes.";

                //confirm the start deley passed in is within the valid range
                //The valid range is between 1 minute and 10 minutes
                if (value < ONE_MINUTE_MILLISECONDS || value > TEN_MINUTES_MILLISECONDS)
                    throw new InvalidModelException(errorMSG, ErrorCodes.MODELINVALID_GAME);
                else
                    startDelay = value;
            }
        }




        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsJoinableAtAnytime { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public double Latitude
        {
            get { return latitude; }
            set
            {
                var errorMessage = "Latitude is not within the valid range. Must be -90 to +90";

                if (value >= -90 && value <= 90)
                    latitude = value;

                else
                    throw new InvalidModelException(errorMessage, ErrorCodes.MODELINVALID_PHOTO);
            }
        }
        public double Longitude
        {
            get { return longitude; }
            set
            {
                var errorMessage = "Longitude is not within the valid range. Must be -180 to +180";

                if (value >= -180 && value <= 180)
                    longitude = value;

                else
                    throw new InvalidModelException(errorMessage, ErrorCodes.MODELINVALID_GAME);
            }
        }
        public int Radius
        {
            get { return radius; }
            set
            {
                var errorMessage = "Radius is not within the valid range. Must be a minimum of 10 meters.";

                if (value >= 10)
                    radius = value;

                else
                    throw new InvalidModelException(errorMessage, ErrorCodes.MODELINVALID_GAME);
            }
        }
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
            TimeLimit = ONE_DAY_MILLISECONDS;
            AmmoLimit = 3;
            StartDelay = TEN_MINUTES_MILLISECONDS;
            ReplenishAmmoDelay = TEN_MINUTES_MILLISECONDS;
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
        /// Creates a game with default values and sets the GameCode
        /// </summary>
        /// <param name="gameCode">The 6 digit game code of the game.</param>
        public Game(string gameCode, int timeLimit, int ammoLimit, int startDelay, int replenishAmmoDelay, string gameMode, bool isJoinableAtAnyTime, double latitude, double longitude, int radius) : this()
        {
            GameCode = gameCode;
            TimeLimit = timeLimit;
            AmmoLimit = ammoLimit;
            StartDelay = startDelay;
            ReplenishAmmoDelay = replenishAmmoDelay;
            GameMode = gameMode;
            IsJoinableAtAnytime = isJoinableAtAnyTime;
            Latitude = latitude;
            Longitude = longitude;
            Radius = radius;
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

        public int CalculateDisabledTime()
        {
            return 60000;
        }

        public double CalculateRadius()
        {
            try
            {
                //Get the number of milliseconds between the start and end time
                TimeSpan span = EndTime.Value.Subtract(StartTime.Value);
                var totalTime = span.TotalMilliseconds;

                //Get the time between the current time and the end time
                span = EndTime.Value.Subtract(DateTime.Now);
                var timeRemaining = span.TotalMilliseconds;

                //Calculate the percentage of time remaining
                var percentage = timeRemaining / totalTime;

                //Return the current radius
                return Radius * percentage;
            }
            catch
            {
                return -1;
            }
        }





        public bool IsInZone(double latitude, double longitude)
        {
            var centerLocation = new GeoCoordinate(Latitude, Longitude);
            var playerLocation = new GeoCoordinate(latitude, longitude);

            //Get the distance in meters
            var distanceBetweenCoords = centerLocation.GetDistanceTo(playerLocation);

            //Get the current radius of the zone
            double currentRadius = CalculateRadius();

            //If the distance between the two points is greater than the radius the player is outside the zone
            if (distanceBetweenCoords > currentRadius)
                return false;
            else
                return true;
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
