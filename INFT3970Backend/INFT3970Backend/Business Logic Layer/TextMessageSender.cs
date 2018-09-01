using System;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace INFT3970Backend.Business_Logic_Layer
{
    public class TextMessageSender
    {
        private const string AccountSID = "AC2e1f821fcbc8308933a9ca301a0127e6";
        private const string AuthToken = "67f5a9b729e21d1bdafef40a8f55b6d9";
        private const string TwilioNumber = "+61488843010";

        public string Message { get; set; }
        public string SendTo { get; set; }

        public TextMessageSender()
        {
            Message = "";
            SendTo = "";
        }

        public TextMessageSender(string sendTo)
        {
            Message = "";
            SendTo = sendTo;
        }


        public TextMessageSender(string message, string sendTo)
        {
            Message = message;
            SendTo = sendTo;
        }


        public bool Send()
        {
            try
            {
                TwilioClient.Init(AccountSID, AuthToken);

                //Defaulting SendTo to send to Jono's phone number because the trial account can only send to this number
                SendTo = "+61457558322";

                var message = MessageResource.Create(
                body: Message,
                from: new PhoneNumber(TwilioNumber),
                to: new PhoneNumber(SendTo)
                );
                return true;
            }
            catch
            {
                return false;
            }
        }


        public bool SendVerificationCode(int verificationCode)
        {
            Message = "Your CamTag verification code is: " + verificationCode;
            return Send();
        }
    }
}
