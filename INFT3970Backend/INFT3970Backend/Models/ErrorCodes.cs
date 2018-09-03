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
        public const int EC_VERIFICATIONCODESENDERROR = 10;
        public const int EC_BUILDMODELERROR = 11;
        public const int EC_PLAYERIDDOESNOTEXIST = 12;
        public const int EC_GAMEDOESNOTEXIST = 13;
        public const int EC_ITEMALREADYEXISTS = 14;

        //PlayerController - Join Game Error Codes
        public const int EC_JOINGAME_INVALIDGAMECODE = 1000;
        public const int EC_JOINGAME_NICKNAMEBLANK = 1001;
        public const int EC_JOINGAME_NICKNAMEINVALID = 1002;
        public const int EC_JOINGAME_CONTACTINVALID = 1003;
        public const int EC_JOINGAME_GAMEALREADYCOMPLETE = 1004;
        public const int EC_JOINGAME_NICKNAMETAKEN = 1005;
        public const int EC_JOINGAME_PHONETAKEN = 1006;
        public const int EC_JOINGAME_EMAILTAKEN = 1007;

    }
}
