using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Backend.Models
{
    public class ErrorCodes
    {
        //General / Global Error Codes
        public const int EC_DATABASECONNECTERROR = 0;
        public const int EC_INSERTERROR = 2;
        public const int EC_GAMENOTACTIVE = 9;
        public const int EC_PLAYERNOTACTIVE = 10;
        public const int EC_BUILDMODELERROR = 11;
        public const int EC_PLAYERDOESNOTEXIST = 12;
        public const int EC_GAMEDOESNOTEXIST = 13;
        public const int EC_ITEMALREADYEXISTS = 14;
        public const int EC_MISSINGORBLANKDATA = 15;
        public const int EC_GAMEALREADYCOMPLETE = 16;
        public const int EC_DATAINVALID = 17;

        //PlayerController - Join Game Error Codes
        public const int EC_JOINGAME_INVALIDGAMECODE = 1000;
        public const int EC_JOINGAME_NICKNAMEINVALID = 1002;
        public const int EC_JOINGAME_CONTACTINVALID = 1003;
        public const int EC_JOINGAME_GAMEALREADYCOMPLETE = 1004;
        public const int EC_JOINGAME_NICKNAMETAKEN = 1005;
        public const int EC_JOINGAME_PHONETAKEN = 1006;
        public const int EC_JOINGAME_EMAILTAKEN = 1007;
        public const int EC_JOINGAME_UNABLETOJOIN = 1008;


        //PlayerController - Verify Player / Verify verification code Error Codes
        public const int EC_VERIFYPLAYER_CODEINVALID = 2000;
        public const int EC_VERIFYPLAYER_CODEINCORRECT = 2001;
        public const int EC_VERIFYPLAYER_ALREADYVERIFIED = 2002;


        //PhotoController - Vote On Photo Error Codes
        public const int EC_VOTEPHOTO_VOTERECORDDOESNOTEXIST = 4000;
        public const int EC_VOTEPHOTO_VOTEALREADYCOMPLETE = 4001;
        public const int EC_VOTEPHOTO_VOTEFINISHTIMEPASSED = 4003;


        //GameController - GetAllPlayersInGame Error Codes
        public const int EC_PLAYERLIST_EMPTYLIST = 5000;
    }
}
