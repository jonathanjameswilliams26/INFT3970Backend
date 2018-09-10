using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace INFT3970Backend.Business_Logic_Layer
{
    public static class EmailSender
    {
        private const string From = "team6.camtag@gmail.com";
        private const string FromName = "CamTag";
        private const string Username = "team6.camtag";
        private const string Password = "admin123!";


        public static bool Send(string sendTo, string subject, string body, bool isHTML)
        {
            SmtpClient SmtpClient;
            MailMessage Mail;
            MailAddress Address;
            SmtpClient = new SmtpClient();
            Mail = new MailMessage();
            Address = new MailAddress(From, FromName);
            SmtpClient.Host = "smtp.gmail.com";
            SmtpClient.Port = 587;
            SmtpClient.EnableSsl = true;
            SmtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            SmtpClient.UseDefaultCredentials = false;
            SmtpClient.Credentials = new NetworkCredential(Username, Password);
            bool isSuccessful = false;

            //Try and send the email to the destination email using the subject and body.
            //If any exception occurs return false
            try
            {
                Mail.IsBodyHtml = isHTML;
                Mail.From = Address;
                Mail.To.Add(new MailAddress(sendTo));
                Mail.Subject = subject;
                Mail.Body = body;
                SmtpClient.Send(Mail);
                isSuccessful = true;
            }
            catch
            {
                isSuccessful = false;
            }

            return isSuccessful;
        }




        public static void SendInBackground(string sendTo, string subject, string body, bool isHTML)
        {
            BackgroundJob.Enqueue(() => Send(sendTo, subject, body, isHTML));
        }
    }
}
