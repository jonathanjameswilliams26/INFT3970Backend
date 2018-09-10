using Hangfire;
using System;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace INFT3970Backend.Business_Logic_Layer
{
    public static class TextMessageSender
    {
        private const string AccountSID = "AC2e1f821fcbc8308933a9ca301a0127e6";
        private const string AuthToken = "67f5a9b729e21d1bdafef40a8f55b6d9";
        private const string TwilioNumber = "+61488843010";


        public static bool Send(string messageText, string sendTo)
        {
            try
            {
                TwilioClient.Init(AccountSID, AuthToken);

                //Defaulting SendTo to send to Jono's phone number because the trial account can only send to this number
                sendTo = "+61457558322";

                var message = MessageResource.Create(
                body: messageText,
                from: new PhoneNumber(TwilioNumber),
                to: new PhoneNumber(sendTo)
                );
                return true;
            }
            catch
            {
                return false;
            }
        }


        public static void SendInBackground(string msgTxt, string sendTo)
        {
            BackgroundJob.Enqueue(() => Send(msgTxt, sendTo));
        }
    }
}
