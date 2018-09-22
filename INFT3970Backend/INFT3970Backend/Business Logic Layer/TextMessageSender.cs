using System;
using System.Threading;
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
                TwilioClient.Init(AccountSID, AuthToken);
                var message = MessageResource.Create(
                body: messageText,
                from: "CamTag",
                to: new PhoneNumber(sendTo)
                );
                Console.WriteLine("Text Message Successfully Sent...");
                return true;
            }
            catch
            {
                Console.WriteLine("An error occurred while trying to send the text message...");
                return false;
            }
        }


        public static void SendInBackground(string msgTxt, string sendTo)
        {
            Thread txtMsgThread = new Thread(
                () => Send(msgTxt, sendTo)
                );
            txtMsgThread.Start();
        }
    }
}
