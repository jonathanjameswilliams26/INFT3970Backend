using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace INFT3970Backend.Business_Logic_Layer
{
    public class EmailSender
    {
        private SmtpClient SmtpClient;
        private MailMessage Mail;
        private MailAddress Address;
        private const string From = "team6.camtag@gmail.com";
        private const string FromName = "CamTag";
        private const string Username = "team6.camtag";
        private const string Password = "admin123!";

        public string SendTo { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsHTML { get; set; }


        public EmailSender()
        {
            SmtpClient = new SmtpClient();
            Mail = new MailMessage();
            Address = new MailAddress(From, FromName);
            SmtpClient.Host = "smtp.gmail.com";
            SmtpClient.Port = 587;
            SmtpClient.EnableSsl = true;
            SmtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            SmtpClient.UseDefaultCredentials = false;
            SmtpClient.Credentials = new NetworkCredential(Username, Password);
        }


        public EmailSender(string sendTo, string subject, string body, bool isHtml)
        {
            SmtpClient = new SmtpClient();
            Mail = new MailMessage();
            Address = new MailAddress(From, FromName);
            SmtpClient.Host = "smtp.gmail.com";
            SmtpClient.Port = 587;
            SmtpClient.EnableSsl = true;
            SmtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            SmtpClient.UseDefaultCredentials = false;
            SmtpClient.Credentials = new NetworkCredential(Username, Password);
            SendTo = sendTo;
            Subject = subject;
            Body = body;
            IsHTML = isHtml;
        }


        /// <summary>
        /// This method sends an email to the destination email address with the specified subject and body set in the properites of the class.
        /// </summary>
        /// <returns>
        /// TRUE if the email sends successfully
        /// FALSE if an error occurs
        /// </returns>
        public bool SendEmail()
        {
            bool isSuccessful = false;

            //Try and send the email to the destination email using the subject and body.
            //If any exception occurs return false
            try
            {
                Mail.IsBodyHtml = IsHTML;
                Mail.From = Address;
                Mail.To.Add(new MailAddress(SendTo));
                Mail.Subject = Subject;
                Mail.Body = Body;
                SmtpClient.Send(Mail);
                isSuccessful = true;
            }
            catch
            {
                isSuccessful = false;
            }

            return isSuccessful;
        }





        public bool SendVerificationEmail(int verificationCode, string sendTo)
        {
            bool isSuccessful = false;

            Body = "Your CamTag verification code is: " + verificationCode;
            Subject = "CamTage Verification Code";

            //Try and send the email to the destination email using the subject and body.
            //If any exception occurs return false
            try
            {
                Mail.IsBodyHtml = IsHTML;
                Mail.From = Address;
                Mail.To.Add(new MailAddress(sendTo));
                Mail.Subject = Subject;
                Mail.Body = Body;
                SmtpClient.Send(Mail);
                isSuccessful = true;
            }
            catch
            {
                isSuccessful = false;
            }

            return isSuccessful;
        }
    }
}
