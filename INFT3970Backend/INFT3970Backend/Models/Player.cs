using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using INFT3970Backend.Models.Errors;

namespace INFT3970Backend.Models
{
    public class Player
    {
        //Private backing stores for public properties
        private int playerID;
        private string nickname;
        private string phone;
        private string email;
        private string selfieDataUrl;
        private int ammoCount;
        private int numKills;
        private int numDeaths;
        private int numPhotosTaken;
        private int gameID;


        public int PlayerID
        {
            get { return playerID; }
            set
            {
                if (value == -1 || value >= 100000)
                    playerID = value;

                else
                    throw new InvalidModelException(ErrorMessages.EM_PLAYER_ID, ErrorCodes.EC_PLAYER_ID);
            }
        }



        public string Nickname
        {
            get { return nickname; }
            set
            {
                if(value == "empty")
                {
                    nickname = value;
                    return;
                }

                if (string.IsNullOrWhiteSpace(value))
                    throw new InvalidModelException(ErrorMessages.EM_PLAYER_NICKNAME, ErrorCodes.EC_PLAYER_NICKNAME);

                var nicknameRegex = new Regex(@"^[a-zA-Z0-9]{1,}$");
                var isMatch = nicknameRegex.IsMatch(value);
                if (!isMatch)
                    throw new InvalidModelException(ErrorMessages.EM_PLAYER_NICKNAME, ErrorCodes.EC_PLAYER_NICKNAME);
                else
                    nickname = value;
            }
        }



        public string Phone
        {
            get { return phone; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    phone = null;
                    return;
                }
                    
                if (!IsPhone(value))
                    throw new InvalidModelException(ErrorMessages.EM_PLAYER_PHONE, ErrorCodes.EC_PLAYER_PHONE);
                else
                    phone = value;
            }
        }




        public string Email
        {
            get { return email; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    email = null;
                    return;
                }

                if (!IsEmail(value))
                    throw new InvalidModelException(ErrorMessages.EM_PLAYER_PHONE, ErrorCodes.EC_PLAYER_PHONE);
                else
                {
                    //If the address is a gmail address then remove the periods because they dont matter.
                    var tempEmail = value;
                    if (value.Contains("@gmail.com"))
                    {
                        string[] emailNoPeriods = value.Split('@');
                        emailNoPeriods[0] = emailNoPeriods[0].Replace(".", "");
                        tempEmail = emailNoPeriods[0] + "@" + emailNoPeriods[1];
                    }
                    email = tempEmail;
                }
            }
        }



        public string SelfieDataURL
        {
            get { return selfieDataUrl; }
            set
            {
                if (value == "empty")
                {
                    selfieDataUrl = value;
                    return;
                }

                if (string.IsNullOrWhiteSpace(value))
                    throw new InvalidModelException(ErrorMessages.EM_PLAYER_SELFIE, ErrorCodes.EC_PLAYER_SELFIE);

                //Confirm the imgURL is a base64 string
                try
                {
                    if (!value.Contains("data:image/jpeg;base64,"))
                        throw new InvalidModelException(ErrorMessages.EM_PLAYER_SELFIE, ErrorCodes.EC_PLAYER_SELFIE);
                    var base64Data = value.Replace("data:image/jpeg;base64,", "");
                    var byteData = Convert.FromBase64String(base64Data);
                    selfieDataUrl = value;
                }
                catch
                {
                    throw new InvalidModelException(ErrorMessages.EM_PLAYER_SELFIE, ErrorCodes.EC_PLAYER_SELFIE);
                }
            }
        }



        public int AmmoCount
        {
            get { return ammoCount; }
            set
            {
                if (value < 0)
                    throw new InvalidModelException(ErrorMessages.EM_PLAYER_AMMO, ErrorCodes.EC_PLAYER_AMMO);
                else
                    ammoCount = value;
            }
        }



        public int NumKills
        {
            get { return numKills; }
            set
            {
                if (value < 0)
                    throw new InvalidModelException(ErrorMessages.EM_PLAYER_KILLS, ErrorCodes.EC_PLAYER_KILLS);
                else
                    numKills = value;
            }
        }



        public int NumDeaths
        {
            get { return numDeaths; }
            set
            {
                if (value < 0)
                    throw new InvalidModelException(ErrorMessages.EM_PLAYER_DEATHS, ErrorCodes.EC_PLAYER_DEATHS);
                else
                    numDeaths = value;
            }
        }



        public int NumPhotosTaken
        {
            get { return numPhotosTaken; }
            set
            {
                if (value < 0)
                    throw new InvalidModelException(ErrorMessages.EM_PLAYER_PHOTOS, ErrorCodes.EC_PLAYER_PHOTOS);
                else
                    numPhotosTaken = value;
            }
        }



        public bool IsHost { get; set; }



        public bool IsVerified { get; set; }



        public bool IsActive { get; set; }



        public bool IsDeleted { get; set; }



        public string ConnectionID { get; set; }



        public bool IsConnected
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ConnectionID))
                    return false;
                else
                    return true;
            }
        }



        public bool HasLeftGame { get; set; }



        public int GameID
        {
            get { return playerID; }
            set
            {
                if (value == -1 || value >= 100000)
                    gameID = value;

                else
                    throw new InvalidModelException(ErrorMessages.EM_PLAYER_GAMEID, ErrorCodes.EC_PLAYER_GAMEID);
            }
        }



        public Game Game { get; set; }
        

        public Player()
        {
            PlayerID = -1;
            GameID = -1;
            Nickname = "empty";
            SelfieDataURL = "empty";
        }


        public Player(int playerID)
        {
            PlayerID = playerID;
            GameID = -1;
            Nickname = "empty";
            SelfieDataURL = "empty";
        }


        public Player(string nickname, string selfieDataURL, string contact)
        {
            PlayerID = -1;
            Nickname = nickname;
            SelfieDataURL = selfieDataURL;
            GameID = -1;

            if (IsPhone(contact))
                Phone = "+61" + contact.Substring(1);

            else
                Email = contact;
        }

        

        //public Response Validate()
        //{
        //    string msg = "";
        //    int code = 1;

        //    //Validate the PlayerID
        //    if (PlayerID != -1 && PlayerID < 100000)
        //        return new Response("PlayerID is not valid. Must be greater than 99999.", ErrorCodes.EC_PLAYER_ID);

        //    //Validate the Nickname
        //    if (!IsNicknameValid(ref msg, ref code))
        //        return new Response(msg, code);
        //}

        

        private bool IsNicknameValid(ref string errorMSG, ref int errorCode)
        {
            errorMSG = string.Empty;
            errorCode = 1;
            var nicknameRegex = new Regex(@"^[a-zA-Z0-9]{1,}$");
            var isMatch = nicknameRegex.IsMatch(Nickname);
            if(!isMatch)
            {
                errorMSG = "The nickname is invalid, please only enter letters and numbers(no spaces).";
                errorCode = ErrorCodes.EC_PLAYER_NICKNAME;
                return false;
            }
            return true;
        }

        public bool HasEmail()
        {
            if (string.IsNullOrWhiteSpace(Email))
                return false;
            else
                return true;
        }



        public bool HasPhone()
        {
            if (string.IsNullOrWhiteSpace(Phone))
                return false;
            else
                return true;
        }

        public static bool IsEmail(string contact)
        {
            Regex emailRegex = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
            return emailRegex.IsMatch(contact);
        }

        public static bool IsPhone(string contact)
        {
            var isNormalPhone = false;
            var isTwilioPhone = false;

            //Check to see if the phone number is a normal 04 number
            var normalPhoneRegex = new Regex(@"^[0-9]{10,10}$");
            isNormalPhone = normalPhoneRegex.IsMatch(contact);

            //Check to see if the phone number is a Twilio +61 number
            var twilioPhoneRegex = new Regex(@"^\+61[0-9]{9,9}$");
            isTwilioPhone = twilioPhoneRegex.IsMatch(contact);

            return isNormalPhone || isTwilioPhone;
        }

        public string GetContact()
        {
            if (string.IsNullOrWhiteSpace(Phone))
                return Email;
            else
                return Phone;
        }
    }
}
