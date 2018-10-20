using System;
using System.Text.RegularExpressions;
using INFT3970Backend.Helpers;
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
                var errorMSG = "PlayerID is invalid. Must be atleast 100000.";

                if (value == -1 || value >= 100000)
                    playerID = value;

                else
                    throw new InvalidModelException(errorMSG, ErrorCodes.MODELINVALID_PLAYER);
            }
        }



        public string Nickname
        {
            get { return nickname; }
            set
            {
                var errorMSG = "The nickname you entered is invalid, please only enter letters and numbers (no spaces).";

                if (string.IsNullOrWhiteSpace(value))
                    throw new InvalidModelException(errorMSG, ErrorCodes.MODELINVALID_PLAYER);

                var nicknameRegex = new Regex(@"^[a-zA-Z0-9]{1,}$");
                var isMatch = nicknameRegex.IsMatch(value);
                if (!isMatch)
                    throw new InvalidModelException(errorMSG, ErrorCodes.MODELINVALID_PLAYER);
                else
                    nickname = value;
            }
        }



        public string Phone
        {
            get { return phone; }
            set
            {
                var errorMSG = "Phone number is invalid format.";

                if (string.IsNullOrWhiteSpace(value))
                {
                    phone = null;
                    return;
                }
                    
                if (!IsPhone(value))
                    throw new InvalidModelException(errorMSG, ErrorCodes.MODELINVALID_PLAYER);
                else
                    phone = value;
            }
        }




        public string Email
        {
            get { return email; }
            set
            {
                var errorMSG = "Email is invalid format.";

                if (string.IsNullOrWhiteSpace(value))
                {
                    email = null;
                    return;
                }

                if (!IsEmail(value))
                    throw new InvalidModelException(errorMSG, ErrorCodes.MODELINVALID_PLAYER);
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
                var errorMSG = "DataURL is not a base64 string.";

                if (value == "empty")
                {
                    selfieDataUrl = value;
                    return;
                }

                if (string.IsNullOrWhiteSpace(value))
                    throw new InvalidModelException(errorMSG, ErrorCodes.MODELINVALID_PLAYER);

                //Confirm the imgURL is a base64 string
                try
                {
                    if (!value.Contains("data:image/jpeg;base64,"))
                        throw new InvalidModelException(errorMSG, ErrorCodes.MODELINVALID_PLAYER);
                    var base64Data = value.Replace("data:image/jpeg;base64,", "");
                    var byteData = Convert.FromBase64String(base64Data);
                    selfieDataUrl = value;
                }
                catch
                {
                    throw new InvalidModelException(errorMSG, ErrorCodes.MODELINVALID_PLAYER);
                }
            }
        }



        public int AmmoCount
        {
            get { return ammoCount; }
            set
            {
                var errorMSG = "Ammo count cannot be less than 0.";

                if (value < 0)
                    throw new InvalidModelException(errorMSG, ErrorCodes.MODELINVALID_PLAYER);
                else
                    ammoCount = value;
            }
        }



        public int NumKills
        {
            get { return numKills; }
            set
            {
                var errorMSG = "Number of kills cannot be less than 0.";

                if (value < 0)
                    throw new InvalidModelException(errorMSG, ErrorCodes.MODELINVALID_PLAYER);
                else
                    numKills = value;
            }
        }



        public int NumDeaths
        {
            get { return numDeaths; }
            set
            {
                var errorMSG = "Number of deaths cannot be less than 0.";

                if (value < 0)
                    throw new InvalidModelException(errorMSG, ErrorCodes.MODELINVALID_PLAYER);
                else
                    numDeaths = value;
            }
        }



        public int NumPhotosTaken
        {
            get { return numPhotosTaken; }
            set
            {
                var errorMSG = "Number of photos taken cannot be less than 0.";

                if (value < 0)
                    throw new InvalidModelException(errorMSG, ErrorCodes.MODELINVALID_PLAYER);
                else
                    numPhotosTaken = value;
            }
        }



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



        public int GameID
        {
            get { return gameID; }
            set
            {
                var errorMSG = "GameID is invalid. Must be atleast 100000.";

                if (value == -1 || value >= 100000)
                    gameID = value;

                else
                    throw new InvalidModelException(errorMSG, ErrorCodes.MODELINVALID_PLAYER);
            }
        }


        public bool IsHost { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string ConnectionID { get; set; }
        public bool HasLeftGame { get; set; }
        public bool IsEliminated { get; set; }
        public bool IsDisabled { get; set; }
        public string PlayerType { get; set; }
        public Game Game { get; set; }
        



        /// <summary>
        /// Creates a Player with the default values
        /// </summary>
        public Player()
        {
            PlayerID = -1;
            GameID = -1;
            Nickname = "Player";
            SelfieDataURL = "empty";
            IsActive = true;
        }





        /// <summary>
        /// Creates a Player with the default values and set the PlayerID
        /// </summary>
        /// <param name="playerID">the ID of the player.</param>
        public Player(int playerID) : this()
        {
            PlayerID = playerID;
        }




        /// <summary>
        /// Creates a Player with the default values and sets the following properties.
        /// </summary>
        /// <param name="nickname">The nickname of the player.</param>
        /// <param name="selfieDataURL">The DataURL of the player's selfie, a base64 string.</param>
        /// <param name="contact">The contact of the player, a phone number or email address.</param>
        public Player(string nickname, string selfieDataURL, string contact) :this()
        {
            Nickname = nickname;
            SelfieDataURL = selfieDataURL;

            if (IsPhone(contact))
                Phone = "+61" + contact.Substring(1);

            else if (IsEmail(contact))
                Email = contact;
            else
                throw new InvalidModelException("Contact is invalid, not a phone number or email address.", ErrorCodes.MODELINVALID_PLAYER);
        }




        /// <summary>
        /// Sends a text or email to the players contact information
        /// </summary>
        /// <param name="msg">The message to send.</param>
        /// <param name="subject">The subject of the message, used when sending emails.</param>
        public void ReceiveMessage(string msg, string subject)
        {
            //Add a footer to the message which includes a link back to the game.
            msg += "\n\nPlease visit theteam6.com to return to your game.";

            if (HasEmail())
                EmailSender.Send(Email, subject, msg, false);
            else
                TextMessageSender.Send(msg, Phone);
        }





        /// <summary>
        /// Checks if the player has an email address.
        /// </summary>
        public bool HasEmail()
        {
            if (string.IsNullOrWhiteSpace(Email))
                return false;
            else
                return true;
        }




        /// <summary>
        /// Checks if the player has a phone number.
        /// </summary>
        public bool HasPhone()
        {
            if (string.IsNullOrWhiteSpace(Phone))
                return false;
            else
                return true;
        }




        /// <summary>
        /// Checks if the contact passed in is a valid email address.
        /// </summary>
        /// <param name="contact">The possible email address.</param>
        public static bool IsEmail(string contact)
        {
            Regex emailRegex = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
            return emailRegex.IsMatch(contact);
        }



        /// <summary>
        /// Checks if the contact passed in is a valid phone number.
        /// </summary>
        /// <param name="contact">The possible phone number.</param>
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



        /// <summary>
        /// Gets the players contact information.
        /// Will return the email address if the player has an email,
        /// otherwise, phone number is returned.
        /// </summary>
        /// <returns></returns>
        public string GetContact()
        {
            if (HasEmail())
                return Email;
            else
                return Phone;
        }




        /// <summary>
        /// Confirms the verification code passed in is valid.
        /// Must be between 10000 and 99999
        /// </summary>
        /// <param name="strCode">The code in string format</param>
        /// <returns>The INT code, negative INT if an error occurred or invalid</returns>
        public static int ValidateVerificationCode(string strCode)
        {
            if (string.IsNullOrWhiteSpace(strCode))
                return -1;

            var code = 0;
            try
            {
                //Confirm the code is within the valid range
                code = int.Parse(strCode);
                if (code < 10000 || code > 99999)
                    throw new Exception();
                return code;
            }
            catch
            {
                return -1;
            }
        }


        /// <summary>
        /// Generates a 5 digit verification code between 10000 and 99999
        /// </summary>
        /// <returns></returns>
        public static int GenerateVerificationCode()
        {
            Random rand = new Random();
            return rand.Next(10000, 99999);
        }


        public bool IsBRPlayer()
        {
            return PlayerType.ToUpper() == "BR";
        }
    }
}
