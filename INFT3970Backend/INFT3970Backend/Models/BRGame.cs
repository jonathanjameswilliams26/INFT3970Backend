using System;
using INFT3970Backend.Models.Errors;

namespace INFT3970Backend.Models
{
    public class BRGame : Game
    {
        //Private backing stores of public properites which have business logic behind them.
        private double lat;
        private double longitude;
        private int radius;

        public double Latitude
        {
            get { return lat; }
            set
            {
                var errorMessage = "Latitude is not within the valid range. Must be -90 to +90";

                if (value >= -90 || value <= 90)
                    lat = value;

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

                if (value >= -180 || value <= 180)
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



        public BRGame(string gameCode, int timeLimit, int ammoLimit, int startDelay, int replenishAmmoDelay, string gameMode, bool isJoinableAtAnyTime, double latitude, double longitude, int radius)
            : base(gameCode, timeLimit, ammoLimit, startDelay, replenishAmmoDelay, gameMode, isJoinableAtAnyTime)
        {
            Latitude = latitude;
            Longitude = longitude;
            Radius = radius;
        }

        public BRGame(Game game)
        {
            GameID = game.GameID;
            GameCode = game.GameCode;
            GameMode = game.GameMode;
            GameState = game.GameState;
            NumOfPlayers = game.NumOfPlayers;
            TimeLimit = game.TimeLimit;
            AmmoLimit = game.AmmoLimit;
            ReplenishAmmoDelay = game.ReplenishAmmoDelay;
            StartDelay = game.StartDelay;
            StartTime = game.StartTime;
            EndTime = game.EndTime;
            IsJoinableAtAnytime = game.IsJoinableAtAnytime;
            IsActive = game.IsActive;
            IsDeleted = game.IsDeleted;
            Players = game.Players;
        }

        public bool IsInZone(double latitude, double longitude)
        {
            //TODO: implement method
            throw new NotImplementedException();
        }
    }
}
