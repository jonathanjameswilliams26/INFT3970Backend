using Hangfire;
using System;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace INFT3970Backend.Business_Logic_Layer
{
    public static class TextMessageSender
    {
        private const string AccountSID = "AC95bc5549490e573dd2ce6941b436d72e";
        private const string AuthToken = "c186c1cbcf2840de60493eaabbce181c";
        private const string TwilioNumber = "+61408929181";


        public static bool Send(string messageText, string sendTo)
        {
            try
            {
                //TwilioClient.Init(AccountSID, AuthToken);

                ////Defaulting SendTo to send to Jono's phone number because the trial account can only send to this number
                //sendTo = "+61457558322";

                //var message = MessageResource.Create(
                //body: messageText,
                //from: new PhoneNumber(TwilioNumber),
                //to: new PhoneNumber(sendTo)
                //);
                return true;
            }
            catch
            {
                return false;
            }
        }


        public static void SendInBackground(string msgTxt, string sendTo)
        {
            //BackgroundJob.Enqueue(() => Send(msgTxt, sendTo));
        }
    }
}
