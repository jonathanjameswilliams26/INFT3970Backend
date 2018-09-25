using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Backend.Models.Errors
{
    public class ErrorMessages
    {
        //Player Class Error Messages - - Invalid Properties
        public const string EM_PLAYER_ID = "PlayerID is invalid. Must be atleast 100000.";
        public const string EM_PLAYER_NICKNAME = "The nickname you entered is invalid, please only enter letters and numbers (no spaces).";
        public const string EM_PLAYER_PHONE = "Phone number is invalid format.";
        public const string EM_PLAYER_EMAIL = "Email is invalid format.";
        public const string EM_PLAYER_SELFIE = "DataURL is not a base64 string.";
        public const string EM_PLAYER_AMMO = "Ammo count cannot be less than 0.";
        public const string EM_PLAYER_KILLS = "Number of kills cannot be less than 0.";
        public const string EM_PLAYER_DEATHS = "Number of deaths cannot be less than 0.";
        public const string EM_PLAYER_PHOTOS = "Number of photos taken cannot be less than 0.";
        public const string EM_PLAYER_GAMEID = "GameID is invalid. Must be atleast 100000.";




        //Game Class Error Messages - - Invalid Properties
        public const string EM_GAME_ID = "GameID is invalid. Must be atleast 100000.";
        public const string EM_GAME_CODE = "GameCode is invalid. Must be 6 characters long and only contain letters and numbers.";
        public const string EM_GAME_PLAYERCOUNT = "Number of players is invalid, cannot be less than 0 or greater than 16.";
        public const string EM_GAME_MODE = "Game mode is invalid.";
        public const string EM_GAME_STATE = "Game state is invalid.";
        public const string EM_GAME_DATES = "The game start time or end time is invalid.";

        //Photo Model Error Message - Invalid Model
        public const string EM_PHOTO_MODELINVALID = "The Photo model is invalid. ";
    }
}
